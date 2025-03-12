using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Extension;
using AEAssist.Helper;
using Dalamud.Game.ClientState.Objects.Types;
using Frost.Frost_WAR.Frost_WAR_Data;

namespace Frost.Frost_WAR.Frost_WAR_SlotResolvers
{
    internal class 前置_立刻战栗 : Frost_WAR_ISlotResolver
    {
        public override int Check()
        {
            if (!QT.GetQt("立刻战栗"))
            {
                return -1;
            }

            int baseCheck = CanUseDefenceOGCD((uint)WARActionID.战栗);
            if (baseCheck == -600)
            {
                return -600;
            }
            if (baseCheck!=0)
            {
                Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("战栗不可用,技能使用失败，QT已自动关闭");
                QT.SetQt("立刻战栗", false);
                return baseCheck;
            }
            double cooldown = SpellHelper.GetSpell((uint)WARActionID.战栗).Cooldown.TotalSeconds;
            if (cooldown > Setting.cd预检测阈值)
            {
                Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("战栗cd中，QT已自动关闭");
                QT.SetQt("立刻战栗", false);
                return -401;
            }
            if(cooldown > Setting.技能提前时间)
            {
                return -400;
            }

            Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("立刻战栗");
            SpellID = (uint)WARActionID.战栗;
            target = Core.Me;
            return base.Check();
        }
    }
}

