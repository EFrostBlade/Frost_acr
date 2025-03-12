using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Extension;
using AEAssist.Helper;
using Dalamud.Game.ClientState.Objects.Types;
using Frost.Frost_WAR.Frost_WAR_Data;

namespace Frost.Frost_WAR.Frost_WAR_SlotResolvers
{
    internal class 前置_立刻雪仇 : Frost_WAR_ISlotResolver
    {
        public override int Check()
        {
            if (!QT.GetQt("立刻雪仇"))
            {
                return -1;
            }

            int baseCheck = CanUseRoleAction((uint)WARActionID.雪仇,true);
            if (baseCheck == -600)
            {
                return -600;
            }
            if (baseCheck!=0)
            {
                Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("雪仇不可用,技能使用失败，QT已自动关闭");
                QT.SetQt("立刻雪仇", false);
                return baseCheck;
            }
            double cooldown = SpellHelper.GetSpell((uint)WARActionID.雪仇).Cooldown.TotalSeconds;
            if (cooldown > Setting.cd预检测阈值)
            {
                Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("雪仇cd中，QT已自动关闭");
                QT.SetQt("立刻雪仇", false);
                return -401;
            }
            if(cooldown > Setting.技能提前时间)
            {
                return -400;
            }

            Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("立刻雪仇");
            SpellID = (uint)WARActionID.雪仇;
            target = Core.Me;
            return base.Check();
        }
    }
}

