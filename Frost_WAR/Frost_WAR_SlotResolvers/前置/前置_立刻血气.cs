using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Extension;
using AEAssist.Helper;
using Dalamud.Game.ClientState.Objects.Types;
using Frost.Frost_WAR.Frost_WAR_Data;

namespace Frost.Frost_WAR.Frost_WAR_SlotResolvers
{
    internal class 前置_立刻血气 : Frost_WAR_ISlotResolver
    {
        public override int Check()
        {
            if (!QT.GetQt("立刻血气"))
            {
                return -1;
            }
            if (Core.Me.Level >= 82)
            {
                int baseCheck = CanUseDefenceOGCD((uint)WARActionID.原初的血气);
                if (baseCheck == -600)
                {
                    return -600;
                }
                if (baseCheck != 0)
                {
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("血气不可用,技能使用失败，QT已自动关闭");
                    QT.SetQt("立刻血气", false);
                    return baseCheck;
                }
                double cooldown = SpellHelper.GetSpell((uint)WARActionID.原初的血气).Cooldown.TotalSeconds;
                if (cooldown > Setting.cd预检测阈值)
                {
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("血气cd中，QT已自动关闭");
                    QT.SetQt("立刻血气", false);
                    return -401;
                }
                if (cooldown > Setting.技能提前时间)
                {
                    return -400;
                }

                Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("立刻血气");
                SpellID = (uint)WARActionID.原初的血气;
                target = Core.Me;
                return base.Check();
            }
            else
            {
                int baseCheck = CanUseDefenceOGCD((uint)WARActionID.原初的直觉);
                if (baseCheck == -600)
                {
                    return -600;
                }
                if (baseCheck != 0)
                {
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("直觉不可用,技能使用失败，QT已自动关闭");
                    QT.SetQt("立刻血气", false);
                    return baseCheck;
                }
                double cooldown = SpellHelper.GetSpell((uint)WARActionID.原初的直觉).Cooldown.TotalSeconds;
                if (cooldown > Setting.cd预检测阈值)
                {
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("直觉cd中，QT已自动关闭");
                    QT.SetQt("立刻血气", false);
                    return -401;
                }
                if (cooldown > Setting.技能提前时间)
                {
                    return -400;
                }

                Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("立刻直觉");
                SpellID = (uint)WARActionID.原初的直觉;
                target = Core.Me;
                return base.Check();
            }
        }
    }
}

