using Lighsing.KingsExecutioner.Cards;
using Lighsing.KingsExecutioner.Cards.Effects;
using System;
using System.Collections.Generic;

namespace Lighsing.KingsExecutioner.Combat
{
    /// <summary>
    /// 表示一场对局。
    /// </summary>
    public class Combatfield
    {
        private static Combatfield _instance;
        private List<CombatParticipant> _participants;
        private CombatRound _round;
        public CombatCardArgs[,] cardSlots;
        /// <summary>
        /// 当前进行中的对局的实例。如果没有正在进行的对局，应当为null。
        /// </summary>
        public static Combatfield Instance
        {
            get => _instance;
            private set
            {
                if (_instance != null)
                {
#if UNITY_EDITOR
                    UnityEngine.Debug.LogWarning("可能存在没有正确结束的对局！");
#endif
                }
                _instance = value;
            }
        }
        /// <summary>
        /// 访问对局中的玩家实例。
        /// </summary>
        public CombatPlayer Player
        {
            get => _participants[0] as CombatPlayer;
        }
        /// <summary>
        /// 对局参与者列表。
        /// </summary>
        public List<CombatParticipant> Participants => _participants;
        /// <summary>
        /// 对局的回合控制。
        /// </summary>
        public CombatRound Round => _round;
        /// <summary>
        /// 新的回合开始时触发。
        /// </summary>
        public event Action<CombatRound> OnNewRoundBegin;
        /// <summary>
        /// 当需要展示玩家摸牌所得时触发。
        /// </summary>
        public event Action<CombatCardArgs> OnDrawCardAnimationRequested;
        /// <summary>
        /// 当敌方上牌动画需要播放时触发。
        /// </summary>
        public event Action<CombatCardArgs, int, int> OnEnemyPlayCardAnimationRequested;
        /// <summary>
        /// 当敌方移动动画需要播放时触发。
        /// </summary>
        public event Action<CombatCardArgs> OnEnemyMoveCardAnimationRequested;

        /// <summary>
        /// 创建一场对局。作为参数的参与者列表会被复制到局内，第一个角色必须是玩家。
        /// </summary>
        /// <param name="participants">参与对局的角色列表</param>
        public Combatfield(List<CombatParticipant> participants)
        {
            if (participants == null || participants.Count <= 0)
            {
                throw (new Exception("创建对局传入了空的 CombatParticipant 列表！"));
            }
            else if (participants[0] is not CombatPlayer)
            {
                throw (new Exception("创建对局传入的 CombatParticipant 列表首位不是玩家！"));
            }
            int timePlayerAppears = 0;
            foreach (CombatParticipant participant in participants)
            {
                if (participant is CombatPlayer)
                {
                    timePlayerAppears++;
                    if (timePlayerAppears > 1)
                    {
                        throw (new Exception("创建对局传入的 CombatParticipant 列表出现多位玩家！"));
                    }
                }
            }
            _instance = this;
            _participants = new List<CombatParticipant>(participants);
            _round = new CombatRound();
            cardSlots = new CombatCardArgs[3, 4];
        }

        /// <summary>
        /// 开始对局。如果敌方存在第0回合上牌的卡牌，则会上牌。
        /// </summary>
        public void CombatBegin()
        {
            InitalizeCards();
            EnemyDeployCards(_round.currentRound, _participants[1].cardsArgs);
            _round.Begin();
            OnNewRoundBegin?.Invoke(_round);
        }

        /// <summary>
        /// 获取指定行和序位的卡牌参数。
        /// </summary>
        public CombatCardArgs GetCardArgsAt(int row, int col)
        {
            return cardSlots[row, col];
        }

        /// <summary>
        /// 设置指定行和序位的卡牌参数。
        /// </summary>
        public void SetCardArgsAt(int row, int col, CombatCardArgs cardArgs)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.Log($"{cardArgs?.Card?.id}被放置于({row},{col})");
#endif
            if (row >= 0 && row < cardSlots.GetLength(0) && col >= 0 && col < cardSlots.GetLength(1))
            {
                if (cardSlots[row, col] != null && cardArgs != null)
                {
                    return;
                }
                if (cardArgs == null)
                {
                    if (cardSlots[row, col] != null)
                    {
                        cardSlots[row, col].row = -1;
                        cardSlots[row, col].col = -1;
                        cardSlots[row, col] = null;
                    }
                    return;
                }
                if (cardArgs.row >= 0 && cardArgs.row < cardSlots.GetLength(0) && cardArgs.col >= 0 && cardArgs.col < cardSlots.GetLength(1))
                {
                    cardSlots[cardArgs.row, cardArgs.col] = null;
                }
                cardSlots[row, col] = cardArgs;
                if (cardArgs.Card.BelongsTo.force == CombatForce.Enemy)
                {
                    cardArgs.Card.name = EffectFactory.Name(cardArgs.Card.noneEffectConfig);
                    cardArgs.introduction = EffectFactory.Introduce(cardArgs.Card.noneEffectConfig);
                }
                if (cardArgs.inPile || cardArgs.InHand)
                {
                    _round.AddEffectFromCard(cardArgs);
                }
                cardArgs.row = row;
                cardArgs.col = col;
                cardArgs.inPile = false;
                cardArgs.InHand = false;
                if (cardArgs.Card is IDefender defender && defender.DefenseValue == 0)
                {
                    defender.Defend(null, 0);
                }
            }
        }

        /// <summary>
        /// 开始新的回合。敌方待战区（第1行）的卡牌会在回合开始时上到战斗区（第2行）。若该列已有卡牌，则暂时保留在待战区。
        /// </summary>
        public void NextRound()
        {
            for (int col = 0; col < 4; col++)
            {
                CombatCardArgs waitingCard = cardSlots[0, col];
                if (waitingCard != null)
                {
                    if (cardSlots[1, col] == null && waitingCard.Card.BelongsTo.force == CombatForce.Enemy)
                    {
                        SetCardArgsAt(1, col, waitingCard);
                        OnEnemyMoveCardAnimationRequested?.Invoke(waitingCard);
                    }
                }
            }
            EnemyDeployCards(_round.currentRound, _participants[1].cardsArgs);
            _round.Begin();
            OnNewRoundBegin?.Invoke(_round);
        }
        /// <summary>
        /// 在指定回合为敌方上牌至待战区。
        /// </summary>
        /// <param name="currentRound">当前回合数</param>
        /// <param name="enemyCards">待出场的敌方卡牌列表</param>
        public void EnemyDeployCards(int currentRound, List<CombatCardArgs> enemyCards)
        {
            List<CombatCardArgs> cardsToDeploy = enemyCards.FindAll(c => c.releaseRound == currentRound);
            foreach (CombatCardArgs card in cardsToDeploy)
            {
                int[] columnWeights = new int[4];
                for (int col = 0; col < 4; col++)
                {
                    columnWeights[col] = (col == 0 || col == 3) ? 2 : 4;
                    if (cardSlots[1, col] != null)
                    {
                        columnWeights[col] -= 3;
                    }
                    if (cardSlots[0, col] != null)
                    {
                        columnWeights[col] = int.MinValue;
                    }
                }
                int bestCol = -1;
                int maxWeight = int.MinValue;
                for (int col = 0; col < 4; col++)
                {
                    if (columnWeights[col] >= maxWeight && cardSlots[0, col] == null)
                    {
                        maxWeight = columnWeights[col];
                        bestCol = col;
                    }
                }
                if (bestCol == -1)
                {
                    card.releaseRound++;
                    continue;
                }
                card.InHand = true;
                SetCardArgsAt(0, bestCol, card);
                OnEnemyPlayCardAnimationRequested?.Invoke(card, 0, bestCol);
            }
        }

        /// <summary>
        /// 玩家摸牌。
        /// </summary>
        public void PlayerDrawCard(bool fighterCard)
        {
            Random rdm = new Random();
            List<CombatCardArgs> pileCardArgs;
            if (fighterCard)
            {
                pileCardArgs = Player.cardsArgs.FindAll(c => c.inPile).FindAll(c => c.Card is FighterCard);
            }
            else
            {
                pileCardArgs = Player.cardsArgs.FindAll(c => c.inPile).FindAll(c => c.Card is not FighterCard);
            }
            if (pileCardArgs == null || pileCardArgs.Count == 0)
            {
                return;
            }
            CombatCardArgs drawnCard = pileCardArgs[rdm.Next(pileCardArgs.Count)];
            if (!string.IsNullOrEmpty(drawnCard.Card.basicEffectConfig))
            {
                drawnCard.Card.name = EffectFactory.Name(drawnCard.Card.basicEffectConfig);
                drawnCard.introduction = EffectFactory.Introduce(drawnCard.Card.basicEffectConfig);
            }
            else if (!string.IsNullOrEmpty(drawnCard.Card.noneEffectConfig))
            {
                drawnCard.Card.name = EffectFactory.Name(drawnCard.Card.noneEffectConfig);
                drawnCard.introduction = EffectFactory.Introduce(drawnCard.Card.noneEffectConfig);
            }
            drawnCard.inPile = false;
            drawnCard.InHand = true;
            Player.handCardsArgs.Add(drawnCard);
            OnDrawCardAnimationRequested?.Invoke(drawnCard);
        }

        private void InitalizeCards()
        {
            foreach (CombatParticipant cp in _participants)
            {
                foreach (CombatCardArgs cca in cp.cardsArgs)
                {
                    cca.Card.BelongsTo = cp;
                    if (cca.Card is IDefender dfd)
                    {
                        Action<IDefender> handleDefeated = null;
                        CombatCardArgs args = cca;
                    }
                }
            }
        }
    }
}
