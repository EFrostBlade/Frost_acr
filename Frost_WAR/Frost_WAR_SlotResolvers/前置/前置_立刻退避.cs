using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Define;
using AEAssist.Extension;
using AEAssist.Helper;
using AEAssist.MemoryApi;
using Dalamud.Game.ClientState.Objects.Types;
using Frost.Frost_WAR.Frost_WAR_Data;
using System;

namespace Frost.Frost_WAR.Frost_WAR_SlotResolvers
{
    internal class 前置_立刻退避 : Frost_WAR_ISlotResolver
    {
        public override int Check()
        {
            if (!QT.GetQt("立刻退避"))
            {
                return -1;
            }

            int baseCheck = CanUseDefenceOGCD((uint)WARActionID.退避);
            if (baseCheck == -600)
            {
                return -600;
            }
            if (baseCheck != 0)
            {
                Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("退避不可用,技能使用失败，QT已自动关闭");
                QT.SetQt("立刻退避", false);
                return baseCheck;
            }
            double cooldown = SpellHelper.GetSpell((uint)WARActionID.退避).Cooldown.TotalSeconds;
            if (cooldown > Setting.cd预检测阈值)
            {
                Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("退避cd中，QT已自动关闭");
                QT.SetQt("立刻退避", false);
                return -401;
            }
            if (cooldown > Setting.技能提前时间)
            {
                return -400;
            }
            IBattleChara? anotherTank = PartyHelper.GetAnotherTank(Core.Me);
            if (anotherTank != null)
            {
                if (GameObjectExtension.Distance(Core.Me, anotherTank, DistanceMode.IgnoreAll) > 25)
                {
                    return -402;
                }
                if (Core.Resolve<MemApiSpell>().CheckActionInRangeOrLoS((uint)WARActionID.退避, anotherTank))
                {
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("立刻退避st");
                    Core.Resolve<MemApiChatMessage>().Toast2($"立刻退避st", 1, 2000);
                    SpellID = (uint)WARActionID.退避;
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
                foreach (IBattleChara melees in PartyHelper.CastableMelees)
                {
                    if(GameObjectExtension.Distance(Core.Me, melees, DistanceMode.IgnoreAll) > 25)
                    {
                        continue;
                    }
                    if (Core.Resolve<MemApiSpell>().CheckActionInRangeOrLoS((uint)WARActionID.退避, melees))
                    {
                        Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog($"退避{melees.Name}");
                        Core.Resolve<MemApiChatMessage>().Toast2($"退避{melees.Name}", 1, 2000);
                        SpellID = (uint)WARActionID.退避;
                        target = melees;
                        return base.Check();
                    }
                }
                foreach (IBattleChara rangeds in PartyHelper.CastableRangeds)
                {
                    if (GameObjectExtension.Distance(Core.Me, rangeds, DistanceMode.IgnoreAll) > 25)
                    {
                        continue;
                    }
                    if (Core.Resolve<MemApiSpell>().CheckActionInRangeOrLoS((uint)WARActionID.退避, rangeds))
                    {
                        Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog($"退避{rangeds.Name}");
                        Core.Resolve<MemApiChatMessage>().Toast2($"退避{rangeds.Name}", 1, 2000);
                        SpellID = (uint)WARActionID.退避;
                        target = rangeds;
                        return base.Check();
                    }
                }
                foreach (IBattleChara healers in PartyHelper.CastableHealers)
                {
                    if (GameObjectExtension.Distance(Core.Me, healers, DistanceMode.IgnoreAll) > 25)
                    {
                        continue;
                    }
                    if (Core.Resolve<MemApiSpell>().CheckActionInRangeOrLoS((uint)WARActionID.退避, healers))
                    {
                        Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog($"退避{healers.Name}");
                        Core.Resolve<MemApiChatMessage>().Toast2($"退避{healers.Name}", 1, 2000);
                        SpellID = (uint)WARActionID.退避;
                        target = healers;
                        return base.Check();
                    }
                }
                Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("退避无目标!");
                Core.Resolve<MemApiChatMessage>().Toast2("退避无目标!", 1, 2000);
                QT.SetQt("立刻退避", false);
            }
            return -404;
        }
    }
}

