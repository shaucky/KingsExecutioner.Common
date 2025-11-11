using Lighsing.KingsExecutioner.Combat;
using System;

namespace Lighsing.KingsExecutioner.Cards.Effects
{
    /// <summary>
    /// 效果抽象基类。
    /// </summary>
    public abstract class Effect
    {
        private bool _isActive = false;
        protected int _id;
        protected CombatRoundStage _stage;
        protected CombatPlayerState _state = CombatPlayerState.None;
        private CombatCardArgs _caster;
        private CombatCardArgs _target;
        protected string[] _params;
        private Action<CombatCardArgs> handleCardDefeated = null;
        private Action<CombatCardArgs> handleTargetDefeated = null;
        public int roundHolds = -1;
        /// <summary>
        /// 效果是否处于激活状态，激活状态的效果才能生效。初始值为false。
        /// </summary>
        public bool IsActive
        {
            get => (_isActive && _state != CombatPlayerState.None) || _state == CombatPlayerState.None;
            set => _isActive = value;
        }
        /// <summary>
        /// 效果的绑定节点。
        /// </summary>
        public CombatRoundStage Stage
        {
            get { return _stage; }
        }
        /// <summary>
        /// 效果关联的玩家模式（故障、过载等）。
        /// </summary>
        public CombatPlayerState State
        {
            get => _state;
        }
        /// <summary>
        /// 产生该效果的卡牌。可能为null。
        /// </summary>
        public CombatCardArgs Caster
        {
            get { return _caster; }
            set
            {
                if (_caster != null)
                {
                    _caster.OnCardDefeated -= handleCardDefeated;
                    _caster.OnCardPlay -= Affect;
                    _caster.OnCardAttack -= Affect;
                    _caster.OnCardDefend -= Affect;
                    _caster.OnCardDefeated -= Affect;
                }
                _caster = value;
                if (_caster != null)
                {
                    switch (_stage)
                    {
                        case CombatRoundStage.OnCardPlay:
                            _caster.OnCardPlay += Affect;
                            break;
                        case CombatRoundStage.OnCardAttack:
                            _caster.OnCardAttack += Affect;
                            break;
                        case CombatRoundStage.OnCardDefend:
                            _caster.OnCardDefend += Affect;
                            break;
                        case CombatRoundStage.OnCardDefeated:
                            _caster.OnCardDefeated += Affect;
                            break;
                    }
                    _caster.OnCardDefeated += handleCardDefeated;
                }
            }
        }
        /// <summary>
        /// 该效果相关的卡牌。可能为null。
        /// </summary>
        public CombatCardArgs Target
        {
            get { return _target; }
            set
            {
                if (_target != null)
                {
                    _target.OnCardDefeated -= handleTargetDefeated;
                    _target.OnCardPlay -= Affect;
                    _target.OnCardAttack -= Affect;
                    _target.OnCardDefend -= Affect;
                    _target.OnCardDefeated -= Affect;
                }
                _target = value;
                if (_target != null)
                {/*
                    switch (_stage)
                    {
                        case CombatRoundStage.OnTargetPlay:
                            _target.OnCardPlay += Affect;
                            break;
                        case CombatRoundStage.OnTargetAttack:
                            _target.OnCardAttack += Affect;
                            break;
                        case CombatRoundStage.OnTargetDefend:
                            _target.OnCardDefend += Affect;
                            break;
                        case CombatRoundStage.OnTargetDefeated:
                            _target.OnCardDefeated += Affect;
                            break;
                    }*/
                    _target.OnCardDefeated += handleTargetDefeated;
                }
            }
        }

        public Effect(string paramString)
        {
            _params = paramString.Split(',');
            _id = Convert.ToInt32(_params[0]);
            _state = (CombatPlayerState)Convert.ToInt32(_params[1]);
            _stage = (CombatRoundStage)Convert.ToInt32(_params[2]);
            roundHolds = Convert.ToInt32(_params[3]);
            handleCardDefeated = args =>
            {
                _caster = null;
                Combatfield.Instance.Round.RemoveEffect(this);
            };
            handleTargetDefeated = args =>
            {
                _target = null;
                Combatfield.Instance.Round.RemoveEffect(this);
            };
        }
        public abstract void Affect(CombatCardArgs card = null); //NOOP: 需要在子类中实现实际的效果逻辑
    }
}
