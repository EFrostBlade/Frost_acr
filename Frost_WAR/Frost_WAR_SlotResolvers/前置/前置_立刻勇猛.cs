using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Define;
using AEAssist.Extension;
using AEAssist.Helper;
using AEAssist.MemoryApi;
using Dalamud.Game.ClientState.Objects.Types;
using Frost.Frost_WAR.Frost_WAR_Data;

namespace Frost.Frost_WAR.Frost_WAR_SlotResolvers
{
    internal class 前置_立刻勇猛 : Frost_WAR_ISlotResolver
    {
        public override int Check()
        {
            if (!QT.GetQt("立刻勇猛"))
            {
                return -1;
            }

            int baseCheck = CanUseDefenceOGCD((uint)WARActionID.原初的勇猛);
            if (baseCheck == -600)
            {
                return -600;
            }
            if (baseCheck!=0)
            {
                Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("勇猛不可用,技能使用失败，QT已自动关闭");
                QT.SetQt("立刻勇猛", false);
                return baseCheck;
            }
            double cooldown = SpellHelper.GetSpell((uint)WARActionID.原初的勇猛).Cooldown.TotalSeconds;
            if (cooldown > Setting.cd预检测阈值)
            {
                Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("勇猛cd中，QT已自动关闭");
                QT.SetQt("立刻勇猛", false);
                return -401;
            }
            if(cooldown > Setting.技能提前时间)
            {
                return -400;
            }
            IBattleChara? anotherTank = PartyHelper.GetAnotherTank(Core.Me);
            if (anotherTank != null)
            {
                if(GameObjectExtension.Distance(Core.Me, anotherTank, DistanceMode.IgnoreAll)>30)
                {
                    return -402;
                }
                if (Core.Resolve<MemApiSpell>().CheckActionInRangeOrLoS((uint)WARActionID.原初的勇猛, anotherTank))
                {
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("立刻勇猛st");
                    SpellID = (uint)WARActionID.原初的勇猛;
                    target = anotherTank;
                    return base.Check();
                }
                else
                {
                    return -402;
                }
            }
            else
            {
                IBattleChara lowestHpTarget = null;
                float lowestHpPercent = 1f;

                foreach (IBattleChara partyer in PartyHelper.CastableAlliesWithin30)
                {
                    if (partyer.CurrentHpPercent() < lowestHpPercent)
                    {
                        lowestHpPercent = partyer.CurrentHpPercent();
                        lowestHpTarget = partyer;
                    }
                }

                if (lowestHpTarget != null)
                {
                    if (Core.Resolve<MemApiSpell>().CheckActionInRangeOrLoS((uint)WARActionID.原初的勇猛, lowestHpTarget))
                    {
                        Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("立刻勇猛血量最低队友");
                        SpellID = (uint)WARActionID.原初的勇猛;
                        target = lowestHpTarget;
                        return base.Check();
                    }
                }
            }
            return -404;
        }
    }
}

