using Lighsing.KingsExecutioner.Cards;

namespace Lighsing.KingsExecutioner.Combat.Damages
{
    /// <summary>
    /// 常规伤害类。造成攻击方攻击值的伤害。
    /// </summary>
    public class NormalDamage : Damage
    {
        public NormalDamage(IAttacker attacker) : base(attacker)
        {
            //NOOP
        }
        protected override int CalculateHarm(IDefender defender)
        {
            return attacker.AttackValue;
        }
    }
}
