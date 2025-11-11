using Lighsing.KingsExecutioner.Combat.Damages;
using System;

namespace Lighsing.KingsExecutioner.Cards
{
    public interface IDefender
    {
        /// <summary>
        /// 防御值发生变化时触发。
        /// </summary>
        public event Action<IDefender> OnDefenseValueChanged;
        /// <summary>
        /// 卡牌受到攻击时触发事件。其中的伤害值为最终伤害。
        /// </summary>
        public event Action<IDefender, int, IAttacker> OnDefend;
        /// <summary>
        /// 卡牌被击败时触发事件。
        /// </summary>
        public event Action<IDefender> OnDefeated;
        public int ConfigDefenseValue { get; set; }
        public int DefenseValue { get; }
        public int DefenseOffset { get; set; }
        public float DefenseMultiplier { get; set; }
        public bool IsDefeated { get; }
        public void Defend(Damage damage, int harm);
        public void Recover(int healing);
    }
}
