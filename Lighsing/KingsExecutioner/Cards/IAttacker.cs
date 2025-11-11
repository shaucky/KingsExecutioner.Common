using System;

namespace Lighsing.KingsExecutioner.Cards
{
    public interface IAttacker
    {
        /// <summary>
        /// 攻击值发生变化时触发。
        /// </summary>
        public event Action<IAttacker> OnAttackValueChanged;
        /// <summary>
        /// 卡牌进行攻击时触发事件。其中的伤害值为原始伤害。
        /// </summary>
        public event Action<IAttacker, int, IDefender> OnAttack;
        public int ConfigAttackValue { get; set; }
        public int AttackValue { get; }
        public int AttackOffset { get; set; }
        public float AttackMultiplier { get; set; }
        public void Attack(IDefender target);
    }
}
