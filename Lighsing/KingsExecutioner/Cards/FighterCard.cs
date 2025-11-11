using Lighsing.KingsExecutioner.Combat.Damages;
using System;

namespace Lighsing.KingsExecutioner.Cards
{
    /// <summary>
    /// 作战卡牌类。
    /// </summary>
    public class FighterCard : CombatCard, IAttacker, IDefender
    {
        protected bool _isDefeated = false;
        protected int _configAttackValue;
        protected int _currentAttackValue;
        protected int _attackOffset = 0;
        protected float _attackMultiplier = 1f;
        protected int _configDefenseValue;
        protected int _currentDefenseValue;
        protected int _defenseOffset = 0;
        protected float _defenseMultiplier = 1f;

        public bool IsDefeated => _isDefeated;
        public int ConfigAttackValue
        {
            get => _configAttackValue;
            set
            {
                int delta = value - _configAttackValue;
                _configAttackValue = value;
                _currentAttackValue += delta;
                OnAttackValueChanged?.Invoke(this);
            }
        }
        public int ConfigDefenseValue
        {
            get => _configDefenseValue;
            set
            {
                int delta = value - _configDefenseValue;
                _configDefenseValue = value;
                _currentDefenseValue += delta;
                if (DefenseValue <= 0)
                {
                    _currentDefenseValue = 0;
                    _isDefeated = true;
                    OnDefeated?.Invoke(this);
                }
                OnDefenseValueChanged?.Invoke(this);
            }
        }

        public int AttackValue
        {
            get
            {
                int atk = (int)Math.Round(_currentAttackValue * _attackMultiplier + _attackOffset);
                return atk >= 0 ? atk: 0;
            }
        }

        public int DefenseValue
        {
            get
            {
                int dfs = (int)Math.Round(_currentDefenseValue * _defenseMultiplier + _defenseOffset);
                return dfs >= 0 ? dfs : 0;
            }
        }

        public int AttackOffset
        {
            get => _attackOffset;
            set
            {
                _attackOffset = value;
                OnAttackValueChanged?.Invoke(this);
            }
        }

        public int DefenseOffset
        {
            get => _defenseOffset;
            set
            {
                _defenseOffset = value;
                if (DefenseValue <= 0)
                {
                    _currentDefenseValue = 0;
                    _isDefeated = true;
                    OnDefeated?.Invoke(this);
                }
                OnDefenseValueChanged?.Invoke(this);
            }
        }

        public float AttackMultiplier
        {
            get => _attackMultiplier;
            set
            {
                _attackMultiplier = value;
                if (_attackMultiplier < 0f)
                {
                    _attackMultiplier = 0f;
                }
                OnAttackValueChanged?.Invoke(this);
            }
        }

        public float DefenseMultiplier
        {
            get => _defenseMultiplier;
            set
            {
                _defenseMultiplier = value;
                if (_defenseMultiplier < 0f)
                {
                    _defenseMultiplier = 0f;
                }
                if (DefenseValue <= 0)
                {
                    _currentDefenseValue = 0;
                    _isDefeated = true;
                    OnDefeated?.Invoke(this);
                }
                OnDefenseValueChanged?.Invoke(this);
            }
        }

        /// <summary>
        /// 攻击值发生变化时触发。
        /// </summary>
        public event Action<IAttacker> OnAttackValueChanged;
        /// <summary>
        /// 防御值发生变化时触发。
        /// </summary>
        public event Action<IDefender> OnDefenseValueChanged;
        /// <summary>
        /// 卡牌进行攻击时触发事件。其中的伤害值为原始伤害。
        /// </summary>
        public event Action<IAttacker, int, IDefender> OnAttack;
        /// <summary>
        /// 卡牌受到攻击时触发事件。其中的伤害值为最终伤害。
        /// </summary>
        public event Action<IDefender, int, IAttacker> OnDefend;
        /// <summary>
        /// 卡牌被击败时触发事件。
        /// </summary>
        public event Action<IDefender> OnDefeated;

        public FighterCard(int configAttackValue, int configDefenseValue)
        {
            _configAttackValue = configAttackValue;
            _configDefenseValue = configDefenseValue;
            _currentAttackValue = configAttackValue;
            _currentDefenseValue = configDefenseValue;
        }

        public void Attack(IDefender target)
        {
            NormalDamage dmg = new NormalDamage(this);
            int harm = dmg.Hurt(target);
            OnAttack?.Invoke(this, harm, target);
            target?.Defend(dmg, harm);
        }

        public void Defend(Damage damage, int harm = 0)
        {
            int dmg = harm == 0 ? (damage != null ? damage.Hurt(this) : 0) : harm;
            int dv = DefenseValue;
            _currentDefenseValue = (int)Math.Round((dv - dmg - _defenseOffset) / _defenseMultiplier);
            int ndv = DefenseValue;
            OnDefend?.Invoke(this, dv - (ndv >= 0 ? ndv : 0), damage?.attacker);
            if (DefenseValue <= 0)
            {
                _currentDefenseValue = 0;
                _isDefeated = true;
                OnDefeated?.Invoke(this);
            }
            OnDefenseValueChanged?.Invoke(this);
        }
        public void Recover(int healing)
        {
            _currentDefenseValue += healing;
            if (DefenseValue <= 0)
            {
                _currentDefenseValue = 0;
                _isDefeated = true;
                OnDefeated?.Invoke(this);
            }
            OnDefenseValueChanged?.Invoke(this);
        }
    }
}
