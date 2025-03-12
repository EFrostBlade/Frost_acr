using AEAssist;
using AEAssist.CombatRoutine.Module;
using AEAssist.Helper;
using AEAssist.MemoryApi;
using Dalamud.Hooking;

namespace Frost.Frost_PLD.Frost_PLD_Data
{

    public static class SpellUtils
    {
        public static bool CanUseOGCD_NoCombat(uint spellId)
        {
            var battleData = Frost_PLD_BattleData.Instance;
            var QT = Frost_PLD_RotationEntry.JobViewWindow;

            if (Core.Resolve<MemApiSpell>().GetActionState(spellId) == 572)  //释放条件未满足
            {
                return false;
            }
            if (Core.Resolve<MemApiSpell>().GetActionState(spellId) == 573) //尚未学习技能
            {
                return false;
            }

            if (Core.Resolve<MemApiSpell>().GetActionState(spellId) == 579) //状态限制
            {
                return false;
            }


            if (AI.Instance.BattleData.LockSpells.Contains(spellId))
            {
                return false;
            }
            if (battleData.是否能力封印)
            {
                return false;
            }
            if (!Core.Resolve<MemApiSpell>().IsLevelEnough(spellId))
            {
                return false;
            }
            if (SpellHelper.GetSpell(spellId).Charges < 1f)
            {
                return false;
            }
            return true;
        }
    }
    public enum PLDActionID : uint
    {
        冲刺 = 3u,
        /// <summary>
        ///1级战技，连击1
        /// </summary>
        先锋剑 = 9u,
        /// <summary>
        ///4级战技，连击2，58级回1000蓝
        /// </summary> 
        暴乱剑 = 15u,
        ///<summary>
        ///26级战技，连击3，60级变为王权剑
        /// </summary>
        战女神之怒 = 21u,
        ///<summary>
        ///60级战技，连击3，64级神圣魔法效果提高，持续30s，76级附加赎罪剑预备，持续30s
        /// </summary>
        王权剑 = 3539u,
        ///<summary>
        ///76级战技，回400蓝，附加祈告剑预备，持续30s
        /// </summary>
        赎罪剑 = 16460u,
        ///<summary>
        ///76级战技，回400蓝，附加葬送剑预备，持续30s
        /// </summary>
        祈告剑 = 36918u,
        ///<summary>
        ///76级战技，回400蓝
        /// </summary>
        葬送剑 = 36919u,
        ///<summary>
        ///64级远程魔法，吟唱1.5s，25米，耗蓝1000，神圣魔法提升或安魂祈祷时无需吟唱，84级后恢复自身血量
        ///</summary>
        圣灵 = 7384u,


        ///<summary>
        ///2级能力，20s内增伤25%，cd60s，54级附加30s沥血剑预备状态
        ///</summary>
        战逃反应 = 20u,
        ///<summary>
        ///54级战技，消耗沥血剑预备
        /// </summary>
        沥血剑 = 3538u,


        ///<summary>
        ///68级近战能力，附加4层安魂祈祷，所有魔法无需吟唱，持续30s，cd60s，80级附加悔罪预备，持续30s,96级升级为绝对统治
        /// </summary>
        安魂祈祷 = 7383u,
        ///<summary>
        ///96级远程aoe能力，25米，附加4层安魂祈祷，所有魔法无需吟唱，持续30s，cd60s，80级附加悔罪预备，持续30s
        /// </summary>
        绝对统治 = 36921u,
        ///<summary>
        ///80级aoe魔法，消耗悔罪预备，25m，耗蓝1000，消耗安魂祈祷时大幅提高威力，84级回血,90级追加魔法连击
        /// </summary>
        悔罪 = 16459u,
        ///<summary>
        ///80级aoe魔法连击1，25m，耗蓝1000，消耗安魂祈祷时大幅提高威力，84级回血
        /// </summary>
        信念之剑 = 25748u,
        ///<summary>
        ///80级aoe魔法连击2，25m，耗蓝1000，消耗安魂祈祷时大幅提高威力，84级回血
        /// </summary>
        真理之剑 = 25749u,
        ///<summary>
        ///80级aoe魔法连击3，25m，耗蓝1000，消耗安魂祈祷时大幅提高威力，84级回血，100级追加荣耀之剑预备，持续30s
        /// </summary>
        英勇之剑 = 25750u,
        ///<summary>
        ///100级aoe能力，消耗荣耀之剑预备，25m
        /// </summary>
        荣耀之剑 = 36922u,


        ///<summary>
        ///30级单体能力，回500蓝，cd30s，86级升级为偿赎剑
        /// </summary>
        深奥之灵 = 29u,
        ///<summary>
        ///86级aoe能力，回500蓝，cd30s
        /// </summary>
        偿赎剑 = 25747u,
        ///<summary>
        ///50级aoe能力，cd30s，附加dot，持续15s
        /// </summary>
        厄运流转 = 23u,
        ///<summary>
        ///66级突进能力，20米，cd30s，可充能两次
        /// </summary>
        调停 = 16461u,


        ///<summary>
        ///6级aoe战技，aoe连击1
        ///</summary>
        全蚀斩 = 7381u,
        ///<summary
        ///40级aoe战技，aoe连击2，回1000蓝，72级附加神圣魔法效果提高，持续30s
        ///</summary>
        日珥斩 = 16457u,
        ///<summary>
        ///72级自身周围aoe魔法，耗蓝1000，吟唱1.5s，神圣魔法提升或安魂祈祷时无需吟唱，84级后恢复自身血量
        /// </summary>
        圣环 = 16458u,


        ///<summary>
        ///10级战技，6s眩晕
        /// </summary>
        盾牌猛击 = 16u,
        ///<summary>
        ///10级能力，盾姿
        /// </summary>
        钢铁信念 = 28u,

        解除钢铁信念 = 32065,
        ///<summary>
        ///15级远程，20米
        /// </summary>
        投盾 = 24u,

        ///<summary>
        ///38级，减伤30%，持续15s，cd120s，92级升级为极致防御
        /// </summary>
        预警 = 17u,
        ///<summary>
        ///92级，减伤40%，持续15s，cd120s，附加持续15s护盾
        /// </summary>
        极致防御 = 36920u,
        ///<summary>
        ///52级，必定格挡（满级减伤20%），持续10s，cd90s
        /// </summary>
        壁垒 = 22u,
        ///<summary>
        ///35级，减伤15%，持续4s，74级提升至6s，cd5s，消耗50忠义，82级升级为圣盾阵
        /// </summary>
        盾阵 = 3542u,
        ///<summary>
        ///82级，减伤15%，持续4s，减伤15%，持续8s，12s回血，cd5s，消耗50忠义
        /// </summary>
        圣盾阵 = 25746u,
        ///<summary>
        ///62级，队友减伤10%，持续6s，有铁壁或预警时+10%，cd10s，消耗50忠义，82级增加至8s，增加10%减伤，持续4s，12s回血
        /// </summary>
        干预 = 7382u,
        ///<summary>
        ///45级，为队友承受伤害，持续12s，cd120s，消耗50忠义
        /// </summary>
        保护 = 27u,
        ///<summary>
        ///50级，无敌，持续10s，cd420s
        /// </summary>
        神圣领域 = 30u,
        ///<summary>
        ///56级，30m群体10%血护盾，持续30s，cd90s，88级回血
        /// </summary>
        圣光幕帘 = 3540u,
        ///<summary>
        ///58级，回血，对队友释放时自己回血一半，耗蓝2000，吟唱1.5s
        /// </summary>
        深仁厚泽 = 3541u,
        ///<summary>
        ///70级，自身必定格挡，身后队友减伤15%，持续5s，最多持续18s，cd120s
        /// </summary>
        武装戍卫 = 7385u,
        /// <summary>
        ///减伤20%，持续20s，冷却90s，94级提高15%受治疗
        /// </summary>
        铁壁 = 7531u,
        /// <summary>
        ///
        /// </summary>
        挑衅 = 7533u,
        /// <summary>
        ///
        /// </summary>
        雪仇 = 7535u,
        /// <summary>
        ///
        /// </summary>
        退避 = 7537u,
        /// <summary>
        ///
        /// </summary>
        插言 = 7538u,
        /// <summary>
        ///
        /// </summary>
        下踢 = 7540u,
        /// <summary>
        ///
        /// </summary>
        亲疏自行 = 7548u,

    }



}
