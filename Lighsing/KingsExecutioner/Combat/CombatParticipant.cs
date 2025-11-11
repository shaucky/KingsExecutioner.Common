using System;
using System.Collections.Generic;

namespace Lighsing.KingsExecutioner.Combat
{
    /// <summary>
    /// 对局参与者。具备牌组、阵营等字段，以及一个被击败时触发的事件。
    /// </summary>
    public class CombatParticipant
    {
        /// <summary>
        /// 参与者被击败时触发的事件。
        /// </summary>
        public event Action<CombatParticipant> OnDefeated;
        /// <summary>
        /// 参与者所属阵营。
        /// </summary>
        public CombatForce force;
        /// <summary>
        /// 参与者的牌组。
        /// </summary>
        public List<CombatCardArgs> cardsArgs = new List<CombatCardArgs>();

        protected void InvokeOnDefeated()
        {
            OnDefeated?.Invoke(this);
        }
    }
}
