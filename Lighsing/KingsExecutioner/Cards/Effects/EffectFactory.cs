using Lighsing.KingsExecutioner.Combat;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Lighsing.KingsExecutioner.Cards.Effects
{
    /// <summary>
    /// 效果工厂类。
    /// </summary>
    public abstract class EffectFactory
    {
        /// <summary>
        /// 名词替换说明：
        /// 能量=>内存；卡牌、单位=>指令；击杀、消灭=>终止
        /// </summary>
        public static ReadOnlyDictionary<int, KeyValuePair<Type, string>> effects = new ReadOnlyDictionary<int, KeyValuePair<Type, string>>(
            new Dictionary<int, KeyValuePair<Type, string>>()
            {
                { 1, new KeyValuePair<Type, string>(typeof(Effect), "") }
            }
        );
        public static ReadOnlyDictionary<CombatRoundStage, string> stages = new ReadOnlyDictionary<CombatRoundStage, string>(
            new Dictionary<CombatRoundStage, string>()
            {
                { CombatRoundStage.BeforeBegin, "回合开始前" },
                { CombatRoundStage.AfterBegin, "回合开始后" },
                { CombatRoundStage.BeforeEnd, "回合结束前" },
                { CombatRoundStage.AfterEnd, "回合结束后" },
                { CombatRoundStage.OnCardPlay, "登场时" },
                { CombatRoundStage.OnCardAttack, "发动攻击时" },
                { CombatRoundStage.OnCardDefend, "受到攻击时" },
                { CombatRoundStage.OnCardDefeated, "被终止时" },/*
                { CombatRoundStage.OnTargetPlay, "目标登场时" },
                { CombatRoundStage.OnTargetAttack, "目标攻击时" },
                { CombatRoundStage.OnTargetDefend, "目标受到攻击时" },
                { CombatRoundStage.OnTargetDefeated, "目标被击破时" },*/
            }
        );

        /// <summary>
        /// 根据配置数据创建效果。
        /// </summary>
        /// <param name="configString">配置数据</param>
        /// <returns>所创建的效果实例</returns>
        public static Effect CreateEffect(string configString)
        {
            string[] configs = configString.Split(',');
            //int cardId = Convert.ToInt32(configs[0]);//卡ID
            //Type cardType = configs[1];//卡类型（1作战卡2道具卡）
            //string cardName = configs[2];//卡名称——方便的话把导出的CSV从 GB2312 编码转为 UTF-8 
            //int 生命3
            //int 攻击4
            uint uefcId;
            if (!uint.TryParse(configs[5], out uefcId))
            {
                return null;
            }
            else if (uefcId < 0)
            {
                return null;
            }
            int efcId = Convert.ToInt32(uefcId);//效果ID
            //CombatPlayerState state = (CombatPlayerState)Convert.ToInt32(configs[6]);//状态ID（故障等）
            //CombatRoundStage stage = (CombatRoundStage)Convert.ToInt32(configs[7]);//触发节点
            //int roundHolds = Convert.ToInt32(configs[8]);//持续回合数
            return (Effect)Activator.CreateInstance(effects[efcId].Key, string.Join(",", configs.Skip(5)));//变长参数
        }
        public static string Name(string configString)
        {
            string[] configs = configString.Split(',');
            return configs[2];
        }
        public static int Id(string configString)
        {
            string[] configs = configString.Split(',');
            return Convert.ToInt32(configs[0]);
        }
        public static string Introduce(string configString)
        {
            string[] configs = configString.Split(',');
            uint uefcId;
            if (!uint.TryParse(configs[5], out uefcId))
            {
                return string.Empty;
            }
            else if (uefcId < 0)
            {
                return string.Empty;
            }
            int efcId = Convert.ToInt32(uefcId);
            if (!effects.TryGetValue(efcId, out var effectInfo))
            {
                return string.Empty;
            }
            string template = effectInfo.Value;
            string[] parameters = configs.Skip(9).ToArray();
            Dictionary<string, string> replacements = new Dictionary<string, string>();
            for (int i = 0; i < parameters.Length; i++)
            {
                string placeholder = $"%param{i + 1}%";
                string rawValue = parameters[i];
                string displayValue = ConvertParamValue(rawValue, placeholder);
                replacements[placeholder] = displayValue;
            }
            string result = template;
            foreach (KeyValuePair<string, string> kv in replacements)
            {
                result = result.Replace(kv.Key, kv.Value);
            }
            result = $"{stages[(CombatRoundStage)Convert.ToInt32(configs[7])]}\n" + result;
            if (Convert.ToInt32(configs[8]) > 0)
            {
                result = $"{Convert.ToInt32(configs[8])}回合内\n" + result;
            }
            return result;
        }

        /// <summary>
        /// 参数格式转换逻辑。当前仅直接返回数字，未来可以根据占位符、数值或语境进行命名映射。
        /// </summary>
        private static string ConvertParamValue(string rawValue, string placeholder)
        {
            if (int.TryParse(rawValue, out int value))
            {
                if (value > 0)
                {
                    rawValue = "+" + rawValue;
                }
            }
            return rawValue;
        }
    }
}
