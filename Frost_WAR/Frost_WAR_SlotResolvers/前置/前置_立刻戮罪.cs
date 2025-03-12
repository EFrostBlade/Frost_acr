using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Extension;
using AEAssist.Helper;
using Dalamud.Game.ClientState.Objects.Types;
using Frost.Frost_WAR.Frost_WAR_Data;

namespace Frost.Frost_WAR.Frost_WAR_SlotResolvers
{
    internal class 前置_立刻戮罪 : Frost_WAR_ISlotResolver
    {
        public override int Check()
        {
            if (!QT.GetQt("立刻戮罪"))
            {
                return -1;
            }
            if (Core.Me.Level >= 92)
            {
                int baseCheck = CanUseDefenceOGCD((uint)WARActionID.戮罪);
                if (baseCheck == -600)
                {
                    return -600;
                }
                if (baseCheck != 0)
                {
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("戮罪不可用,技能使用失败，QT已自动关闭");
                    QT.SetQt("立刻戮罪", false);
                    return baseCheck;
                }
                double cooldown = SpellHelper.GetSpell((uint)WARActionID.戮罪).Cooldown.TotalSeconds;
                if (cooldown > Setting.cd预检测阈值)
                {
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("戮罪cd中，QT已自动关闭");
                    QT.SetQt("立刻戮罪", false);
                    return -401;
                }
                if (cooldown > Setting.技能提前时间)
                {
                    return -400;
                }

                Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("立刻戮罪");
                SpellID = (uint)WARActionID.戮罪;
                target = Core.Me;
                QT.SetQt("立刻戮罪", false);
                return base.Check();
            }
            else
            {
                int baseCheck = CanUseDefenceOGCD((uint)WARActionID.复仇);
                if (baseCheck == -600)
                {
                    return -600;
                }
                if (baseCheck != 0)
                {
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("复仇不可用,技能使用失败，QT已自动关闭");
                    QT.SetQt("立刻戮罪", false);
                    return baseCheck;
                }
                double cooldown = SpellHelper.GetSpell((uint)WARActionID.复仇).Cooldown.TotalSeconds;
                if (cooldown > Setting.cd预检测阈值)
                {
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("复仇cd中，QT已自动关闭");
                    QT.SetQt("立刻戮罪", false);
                    return -401;
                }
                if (cooldown > Setting.技能提前时间)
                {
                    return -400;
                }

                Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("立刻复仇");
                SpellID = (uint)WARActionID.复仇;
                target = Core.Me;
                return base.Check();
            }
        }
    }
}

