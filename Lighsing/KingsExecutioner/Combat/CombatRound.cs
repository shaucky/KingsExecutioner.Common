using Lighsing.KingsExecutioner.Cards;
using Lighsing.KingsExecutioner.Cards.Effects;
using System;
using System.Collections.Generic;

namespace Lighsing.KingsExecutioner.Combat
{
    /// <summary>
    /// 负责对局的回合流程。
    /// </summary>
    public class CombatRound
    {
        private bool _acceptInput = false;
        public int roundUpperLimit;
        public int currentRound = 0;
        private Dictionary<CombatRoundStage, List<Effect>> _stageEffects = new Dictionary<CombatRoundStage, List<Effect>>()
        {
            { CombatRoundStage.BeforeBegin, new List<Effect>() },
            { CombatRoundStage.AfterBegin, new List<Effect>() },
            { CombatRoundStage.BeforeEnd, new List<Effect>() },
            { CombatRoundStage.AfterEnd, new List<Effect>() },
            { CombatRoundStage.OnCardPlay, new List<Effect>() },
            { CombatRoundStage.OnCardAttack, new List<Effect>() },
            { CombatRoundStage.OnCardDefend, new List<Effect>() },
            { CombatRoundStage.OnCardDefeated, new List<Effect>() },
        };
        private CombatCardArgs[,] CardSlots => Combatfield.Instance.cardSlots;
        /// <summary>
        /// 表示此刻是否接受输入。
        /// </summary>
        public bool AcceptInput => _acceptInput;
        /// <summary>
        /// 当需要展示效果产生的影响时触发。
        /// </summary>
        public event Action<Effect, Action> OnEffectAnimationRequested;
        /// <summary>
        /// 当需要展示攻击产生的影响时触发。
        /// </summary>
        public event Action<CombatCard, CombatCard, Action> OnAttackAnimationRequested;
        public event Action<CombatRound, bool> OnCombatEnded;

        /// <summary>
        /// 该方法触发新的回合开始，补充玩家行动点，触发所有回合开始时效果。
        /// </summary>
        public void Begin()
        {
            if (currentRound == 0)
            {
                Combatfield.Instance.Player.OnPowerChanged += HandlePlayerPowerChanged;
                Combatfield.Instance.Player.OnDefeated += player =>
                {
                    IsGameEnd();
                };
            }
            currentRound++;
            BeforeBegin();
            Combatfield.Instance.Player.Energy += 2;
#if UNITY_EDITOR
            UnityEngine.Debug.Log($"第 {currentRound} 回合开始。");
#endif
        }
        private void BeforeBegin()
        {
            ApplyEffects(_stageEffects[CombatRoundStage.BeforeBegin], AfterBegin);
        }
        private void AfterBegin()
        {
            ApplyEffects(_stageEffects[CombatRoundStage.AfterBegin], AllowPlayerInput);
        }
        private void AllowPlayerInput()
        {
            _acceptInput = true;
        }
        /// <summary>
        /// 结束玩家的输入阶段，开始当回合战斗。
        /// </summary>
        public void EndInput()
        {
#if UNITY_EDITOR
            UnityEngine.Debug.Log($"玩家结束输入。");
#endif
            _acceptInput = false;
            ValidateAndFixCards(Combatfield.Instance.Participants[0].cardsArgs);
            ValidateAndFixCards(Combatfield.Instance.Participants[1].cardsArgs);
            PlayerAction();
        }
        private void PlayerAction()
        {
#if UNITY_EDITOR
            UnityEngine.Debug.Log($"玩家势力行动。");
#endif
            AttackSequence(CombatForce.Player, EnemyAction);
        }
        private void EnemyAction()
        {
#if UNITY_EDITOR
            UnityEngine.Debug.Log($"敌方势力行动。");
#endif
            AttackSequence(CombatForce.Enemy, BeforeEnd);
        }
        private void BeforeEnd()
        {
            ApplyEffects(_stageEffects[CombatRoundStage.BeforeEnd], AfterEnd);
        }
        private void AfterEnd()
        {
            ApplyEffects(_stageEffects[CombatRoundStage.AfterEnd], CheckIsGameEnd);
        }
        private void AttackSequence(CombatForce localForce, Action callback = null)
        {
            int rows = CardSlots.GetLength(0);
            int cols = CardSlots.GetLength(1);
            List<CombatCardArgs> attackers = new List<CombatCardArgs>();
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    CombatCardArgs args = CardSlots[r, c];
                    if (args?.Card.BelongsTo.force == localForce && args.Card is IAttacker)
                    {
                        attackers.Add(args);
                    }
                }
            }
            int index = 0;
            void AttackNext()
            {
                if (index < attackers.Count)
                {
                    CombatCardArgs attackerArgs = attackers[index];
                    index++;
                    IAttacker attacker = attackerArgs.Card as IAttacker;
                    if (attacker != null)
                    {
                        if (attacker is IDefender dfd)
                        {
                            if (dfd.IsDefeated)
                            {
                                AttackNext();
                                return;
                            }
                        }
                        CombatCardArgs[] targetArgs = SelectTargets(attackerArgs.Card.BelongsTo.force, attackerArgs.row, attackerArgs.col);
                        int overflowValue = 0;
                        IDefender defender = null;
                        Action<IAttacker, int, IDefender> atkHandler = null;
                        Action<IDefender, int, IAttacker> dfdHandler = null;
                        atkHandler = (atk, dmg, dfd) =>
                        {
                            overflowValue += dmg;
                            attacker.OnAttack -= atkHandler;
                        };
                        dfdHandler = (dfd, dmg, atk) =>
                        {
                            overflowValue -= dmg;
                            if (defender != null)
                            {
                                defender.OnDefend -= dfdHandler;
                            }
                        };
                        attacker.OnAttack += atkHandler;
                        int idx = 0;
                        if (targetArgs != null && targetArgs.Length > 0)
                        {
                            while (idx < targetArgs.Length)
                            {
                                defender = targetArgs[idx]?.Card as IDefender;
                                if (defender != null)
                                {
                                    defender.OnDefend += dfdHandler;
                                }
                                if (idx == 0)
                                {
                                    attacker.Attack(defender);
                                }
                                else if (defender != null)
                                {
                                    defender.Defend(null, overflowValue);
                                }
                                if (overflowValue <= 0)
                                {
                                    break;
                                }
                                idx++;
                            }
                        }
                        else
                        {
                            attacker.Attack(null);
                        }
                        switch (attackerArgs.Card.BelongsTo.force)
                        {
                            case CombatForce.Player:
                                Combatfield.Instance.Player.Power += overflowValue;
                                break;
                            case CombatForce.Enemy:
                                Combatfield.Instance.Player.Power -= overflowValue;
                                break;
                        }
                        IsGameEnd();
                        OnAttackAnimationRequested?.Invoke(attackerArgs.Card, targetArgs.Length > 0 ? targetArgs[0].Card : null, AttackNext);
                    }
                    else
                    {
                        AttackNext();
                    }
                }
                else
                {
                    callback?.Invoke();
                }
            }
            AttackNext();
        }
        private CombatCardArgs[] SelectTargets(CombatForce force, int row, int col)
        {
            List<CombatCardArgs> targets = new List<CombatCardArgs>();
            int rows = CardSlots.GetLength(0);
            if (force == CombatForce.Player)
            {
                for (int r = row; r >= 0; r--)
                {
                    if (CardSlots[r, col]?.Card != null && CardSlots[r, col].Card.BelongsTo.force != force)
                    {
                        targets.Add(CardSlots[r, col]);
                    }
                }
            }
            else if (force == CombatForce.Enemy)
            {
                for (int r = row; r < rows; r++)
                {
                    if (CardSlots[r, col]?.Card != null && CardSlots[r, col].Card.BelongsTo.force != force)
                    {
                        targets.Add(CardSlots[r, col]);
                    }
                }
            }
            return targets.ToArray();
        }
        private void ApplyEffects(List<Effect> effects, Action callback = null)
        {
            int idx = 0;
            void AffectNext()
            {
                if (effects != null && idx < effects.Count)
                {
                    Effect efc = effects[idx];
                    idx++;
                    try
                    {
                        efc.Affect();
                    }
                    catch (Exception ex)
                    {
                        IsGameEnd();
                        if (efc.roundHolds > 0)
                        {
                            efc.roundHolds--;
                        }
                        AffectNext();
#if UNITY_EDITOR
                        UnityEngine.Debug.LogWarning(ex);
#endif
                    }
                    IsGameEnd();
                    OnEffectAnimationRequested?.Invoke(efc.IsActive ? efc : null, () =>
                    {
                        AffectNext();
                    });
                    if (efc.roundHolds > 0)
                    {
                        efc.roundHolds--;
                    }
                }
                else
                {
                    idx = 0;
                    while (idx < effects.Count)
                    {
                        if (effects[idx].roundHolds == 0)
                        {
                            effects[idx].Caster = null;
                            effects[idx].Target = null;
                            effects.RemoveAt(idx);
                        }
                        else
                        {
                            idx++;
                        }
                    }
                    callback?.Invoke();
                }
            }
            AffectNext();
        }
        private void HandlePlayerPowerChanged(CombatPlayer player)
        {
            if (player.IsError)
            {
                SetEffectActiveByState(CombatPlayerState.Error, true);
            }
            else if (player.IsOverloaded)
            {
                SetEffectActiveByState(CombatPlayerState.Overload, true);
            }
            else
            {
                SetEffectActiveByState(CombatPlayerState.Basic, true);
            }
        }
        public void SetEffectActiveByState(CombatPlayerState state, bool active)
        {
            foreach (KeyValuePair<CombatRoundStage, List<Effect>> kvp in _stageEffects)
            {
                foreach (Effect efc in kvp.Value)
                {
                    if (efc.State == state)
                    {
                        efc.IsActive = active;
                    }
                    else if (efc.State != CombatPlayerState.None)
                    {
                        efc.IsActive = !active;
                    }
                }
            }
        }
        /// <summary>
        /// 根据卡牌创建效果。
        /// </summary>
        /// <param name="cardArgs">要创建效果的卡牌</param>
        public void AddEffectFromCard(CombatCardArgs cardArgs)
        {
            if (!string.IsNullOrEmpty(cardArgs.Card.noneEffectConfig))
            {
                Effect efc = EffectFactory.CreateEffect(cardArgs.Card.noneEffectConfig);
                if (efc == null)
                {
                    return;
                }
                efc.Caster = cardArgs;
                if (_stageEffects.ContainsKey(efc.Stage))
                {
                    _stageEffects[efc.Stage].Add(efc);
                }
                efc.IsActive = true;
            }
            if (!string.IsNullOrEmpty(cardArgs.Card.basicEffectConfig))
            {
                Effect efc = EffectFactory.CreateEffect(cardArgs.Card.basicEffectConfig);
                if (efc == null)
                {
                    return;
                }
                efc.Caster = cardArgs;
                if (_stageEffects.ContainsKey(efc.Stage))
                {
                    _stageEffects[efc.Stage].Add(efc);
                }
                if (!Combatfield.Instance.Player.IsError && !Combatfield.Instance.Player.IsOverloaded)
                {
                    efc.IsActive = true;
                }
            }
            if (!string.IsNullOrEmpty(cardArgs.Card.errorEffectConfig))
            {
                Effect efc = EffectFactory.CreateEffect(cardArgs.Card.errorEffectConfig);
                if (efc == null)
                {
                    return;
                }
                efc.Caster = cardArgs;
                if (_stageEffects.ContainsKey(efc.Stage))
                {
                    _stageEffects[efc.Stage].Add(efc);
                }
                if (Combatfield.Instance.Player.IsError)
                {
                    efc.IsActive = true;
                }
            }
            if (!string.IsNullOrEmpty(cardArgs.Card.overloadEffectConfig))
            {
                Effect efc = EffectFactory.CreateEffect(cardArgs.Card.overloadEffectConfig);
                if (efc == null)
                {
                    return;
                }
                efc.Caster = cardArgs;
                if (_stageEffects.ContainsKey(efc.Stage))
                {
                    _stageEffects[efc.Stage].Add(efc);
                }
                if (Combatfield.Instance.Player.IsOverloaded)
                {
                    efc.IsActive = true;
                }
            }
        }
        /// <summary>
        /// 移除一个效果实例，同时置空它的施放者和目标。
        /// </summary>
        /// <param name="effect">要移除的效果实例</param>
        public void RemoveEffect(Effect effect)
        {
            if (_stageEffects[effect.Stage].Contains(effect))
            {
                _stageEffects[effect.Stage].Remove(effect);
            }
        }
        /// <summary>
        /// 检查并修复所有存活且应已登场的卡牌在战场的引用一致性。
        /// </summary>
        public void ValidateAndFixCards(List<CombatCardArgs> cards)
        {
            CombatCardArgs[,] slots = Combatfield.Instance.cardSlots;
            int rowCount = slots.GetLength(0);
            int colCount = slots.GetLength(1);
            foreach (CombatCardArgs cardArgs in cards)
            {
                IDefender defender = cardArgs.Card as IDefender;
                if (defender != null && !defender.IsDefeated)
                {
                    int row = cardArgs.row;
                    int col = cardArgs.col;
                    if (row >= 0 && row < rowCount && col >= 0 && col < colCount)
                    {
                        if (slots[row, col] != cardArgs)
                        {
                            slots[row, col] = cardArgs;
                        }
                    }
                }
            }
        }
        private void CheckIsGameEnd()
        {
            IsGameEnd();
            Combatfield.Instance.NextRound();
        }
        private bool IsGameEnd()
        {
            if (Combatfield.Instance?.Player?.Power == 0)
            {
                OnCombatEnded?.Invoke(this, false);
                return true;
            }
            bool allEnemyDefeated = true;
            foreach (CombatCardArgs args in Combatfield.Instance.Participants[1].cardsArgs)
            {
                if (args.Card is IDefender defender && !defender.IsDefeated)
                {
                    allEnemyDefeated = false;
                    break;
                }
            }
            if (allEnemyDefeated)
            {
                OnCombatEnded?.Invoke(this, true);
                return true;
            }
            return false;
        }
    }
}
