using System;
using System.Collections.Generic;

namespace Lighsing.KingsExecutioner.Combat
{
    /// <summary>
    /// 表示对局中的玩家。相较于基类，包含玩家相关的字段、属性，例如能量值、行动点和手牌。
    /// </summary>
    public class CombatPlayer : CombatParticipant
    {
        /// <summary>
        /// 玩家的能量值下限。
        /// </summary>
        public static int minPowerValue = 0;
        /// <summary>
        /// 玩家的能量值上限。
        /// </summary>
        public static int maxPowerValue = 12;
        /// <summary>
        /// 进入故障模式的能量值阈值。
        /// </summary>
        public static int errorEnterThreshold = 3;
        /// <summary>
        /// 退出故障模式的能量值阈值。
        /// </summary>
        public static int errorExitThreshold = 3;
        /// <summary>
        /// 退出过载模式的能量值阈值。
        /// </summary>
        public static int overloadExitThreshold = 9;
        /// <summary>
        /// 进入过载模式的能量值阈值。
        /// </summary>
        public static int overloadEnterThreshold = 12;
        private int _currPower;
        private int _currEnergy;
        public List<CombatCardArgs> handCardsArgs;
        public event Action<CombatPlayer> OnPowerChanged;
        public event Action<CombatPlayer> OnEnterError;
        public event Action<CombatPlayer> OnExitError;
        public event Action<CombatPlayer> OnEnterOverload;
        public event Action<CombatPlayer> OnExitOverload;
        public bool IsError { get; private set; } = false;
        public bool IsOverloaded { get; private set; } = false;

        /// <summary>
        /// 玩家的当前能量值。能量值归0时玩家失败，在不同能量下可能进入或退出不同的模式。
        /// </summary>
        public int Power
        {
            get { return _currPower; }
            set
            {
                _currPower = value;
                if (value > maxPowerValue)
                {
                    _currPower = maxPowerValue;
                }
                if (value <= errorEnterThreshold)
                {
                    _currPower = value;
                    if (!IsError)
                    {
                        IsError = true;
                        OnEnterError?.Invoke(this);
                    }
                }
                if (errorExitThreshold < value)
                {
                    _currPower = value;
                    if (IsError)
                    {
                        IsError = false;
                        OnExitError?.Invoke(this);
                    }
                }
                if (value <= overloadExitThreshold)
                {
                    _currPower = value;
                    if (IsOverloaded)
                    {
                        IsOverloaded = false;
                        OnExitOverload?.Invoke(this);
                    }
                }
                if (overloadEnterThreshold <= value)
                {
                    _currPower = overloadEnterThreshold;
                    if (!IsOverloaded)
                    {
                        IsOverloaded = true;
                        OnEnterOverload?.Invoke(this);
                    }
                }
                if (value <= minPowerValue)
                {
                    _currPower = minPowerValue;
                    InvokeOnDefeated();
                }
                OnPowerChanged?.Invoke(this);
            }
        }
        /// <summary>
        /// 玩家的当前行动点。
        /// </summary>
        public int Energy
        {
            get => _currEnergy;
            set => _currEnergy = value;
        }

        public CombatPlayer(int initalPower = 6) : base()
        {
            _currPower = initalPower;
            force = CombatForce.Player;
            handCardsArgs = new List<CombatCardArgs>();
        }
    }
}
