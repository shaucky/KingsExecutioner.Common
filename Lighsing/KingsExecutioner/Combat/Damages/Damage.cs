using Lighsing.KingsExecutioner.Cards;

namespace Lighsing.KingsExecutioner.Combat.Damages
{
    /// <summary>
    /// 伤害抽象基类。伤害类提供可能的伤害值、攻击方等数据。具体的伤害计算逻辑由子类实现。
    /// </summary>
    public abstract class Damage
    {
        public IAttacker attacker;
        /// <summary>
        /// Damage类的构造函数。Demage类及其子类的构造函数一定要有一个attacker参数，attacker可以为null。
        /// </summary>
        /// <param name="attacker">创造伤害的卡牌</param>
        public Damage(IAttacker attacker)
        {
            this.attacker = attacker;
        }

        /// <summary>
        /// 返回对指定卡牌造成的伤害值，不造成实际的伤害。对于存在伤害浮动的子类（例如，概率暴击），每次返回的伤害值可能不同。
        /// </summary>
        /// <param name="defender">被攻击的卡牌</param>
        /// <returns>对目标造成的伤害值</returns>
        public int Hurt(IDefender defender)
        {
            return CalculateHarm(defender);
        }
        protected abstract int CalculateHarm(IDefender defender); //NOOP: 需要在子类中实现实际的伤害逻辑
    }
}
