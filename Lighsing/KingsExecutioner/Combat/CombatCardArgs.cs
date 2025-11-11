using Lighsing.KingsExecutioner.Cards;
using System;

namespace Lighsing.KingsExecutioner.Combat
{
    /// <summary>
    /// 对局卡牌参数类。提供对局内的卡牌各种状态信息。
    /// </summary>
    public class CombatCardArgs
    {
        private CombatCard _card;
        /// <summary>
        /// 指定卡牌目前是否在牌堆内。
        /// </summary>
        public bool inPile = true;
        private bool _inHand = false;
        /// <summary>
        /// 指定卡牌的出场回合。（仅针对敌人卡牌）
        /// </summary>
        public int releaseRound;
        public int row = -1;
        public int col = -1;
        public string introduction = string.Empty;
        /// <summary>
        /// 卡牌打出时触发。
        /// </summary>
        public event Action<CombatCardArgs> OnCardPlay;
        /// <summary>
        /// 卡牌攻击时触发。
        /// </summary>
        public event Action<CombatCardArgs> OnCardAttack;
        /// <summary>
        /// 卡牌受到攻击时触发。
        /// </summary>
        public event Action<CombatCardArgs> OnCardDefend;
        /// <summary>
        /// 卡牌被击破时触发。
        /// </summary>
        public event Action<CombatCardArgs> OnCardDefeated;
        /// <summary>
        /// 指定卡牌目前是否在手中。
        /// </summary>
        public bool InHand
        {
            get {  return _inHand; }
            set
            {
                bool play = false;
                if (_inHand == true && value != _inHand)
                {
                    play = true;
                }
                _inHand = value;
                if (play)
                {
                    OnCardPlay?.Invoke(this);
                }
            }
        }
        /// <summary>
        /// 指向卡牌的实例。
        /// </summary>
        public CombatCard Card => _card;

        public CombatCardArgs(CombatCard card = null)
        {
            _card = card;
            if (_card is IAttacker attacker)
            {
                attacker.OnAttack += (atk, dmg, dfd) => OnCardAttack?.Invoke(this);
            }
            if (_card is IDefender defender)
            {
                defender.OnDefend += (atk, dmg, dfd) => OnCardDefend?.Invoke(this);
                defender.OnDefeated += dfd =>
                {
                    OnCardDefeated?.Invoke(this);
                    Combatfield.Instance.SetCardArgsAt(row, col, null);
                    row = -1;
                    col = -1;
                };
            }
        }
    }
}
