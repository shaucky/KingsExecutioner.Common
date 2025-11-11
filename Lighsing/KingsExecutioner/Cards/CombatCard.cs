using Lighsing.KingsExecutioner.Combat;

namespace Lighsing.KingsExecutioner.Cards
{
    /// <summary>
    /// 可以用于对局的卡牌的抽象基类。
    /// </summary>
    public abstract class CombatCard : CardBase
    {
        /// <summary>
        /// 卡牌ID。
        /// </summary>
        public int id;
        /// <summary>
        /// 卡牌无态效果的配置数据。
        /// </summary>
        public string noneEffectConfig;
        /// <summary>
        /// 卡牌基础效果的配置数据。
        /// </summary>
        public string basicEffectConfig;
        /// <summary>
        /// 卡牌故障效果的配置数据。
        /// </summary>
        public string errorEffectConfig;
        /// <summary>
        /// 卡牌过载效果的配置数据。
        /// </summary>
        public string overloadEffectConfig;
        /// <summary>
        /// 卡牌所有者。
        /// </summary>
        public CombatParticipant BelongsTo { get; set; }
    }
}
