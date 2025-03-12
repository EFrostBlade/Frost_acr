using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Extension;
using AEAssist.Helper;
using AEAssist.MemoryApi;
using Dalamud.Game.ClientState.Objects.Types;
using Frost.Frost_WAR.Frost_WAR_Data;

namespace Frost.Frost_WAR.Frost_WAR_SlotResolvers
{
    internal class 前置_1仇 : Frost_WAR_ISlotResolver
    {
        public override int Check()
        {
            if (!QT.GetQt("1仇"))
            {
                return -1;
            }
            if (Core.Me.GetCurrTarget() == null || BattleData.当前目标 == null)
            {
                return -2;
            }
            if (BattleData.当前目标.GetCurrTarget() == null)
            {
                return -3;
            }

            if (BattleData.当前目标.GetCurrTarget() == Core.Me)
            {
                return -4;
            }
            int baseCheck = CanUseDefenceOGCD((uint)WARActionID.挑衅);
            if (baseCheck != 0)
            {
                Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("挑衅不可用,尝试获取1仇失败");
                return baseCheck;
            }
            double cooldown = SpellHelper.GetSpell((uint)WARActionID.挑衅).Cooldown.TotalSeconds;
            if (cooldown > Setting.技能提前时间)
            {
                Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("挑衅cd中,尝试获取1仇失败");
                return -400;
            }

            Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("挑衅拿1仇");
            Core.Resolve<MemApiChatMessage>().Toast2($"挑衅拿1仇", 1, 2000);
            SpellID = (uint)WARActionID.挑衅;
            target = BattleData.当前目标;
            return base.Check();
        }
    }
}

