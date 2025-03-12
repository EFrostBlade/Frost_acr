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
    internal class 前置_立刻勇猛法系 : Frost_WAR_ISlotResolver
    {
        public override int Check()
        {
            if (!QT.GetQt("立刻勇猛法系"))
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
                QT.SetQt("立刻勇猛法系", false);
                return baseCheck;
            }
            double cooldown = SpellHelper.GetSpell((uint)WARActionID.原初的勇猛).Cooldown.TotalSeconds;
            if (cooldown > Setting.cd预检测阈值)
            {
                Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("勇猛cd中，QT已自动关闭");
                QT.SetQt("立刻勇猛法系", false);
                return -401;
            }
            if(cooldown > Setting.技能提前时间)
            {
                return -400;
            }
            foreach (IBattleChara rangeds in PartyHelper.CastableRangeds)
            {
                if (GameObjectExtension.Distance(Core.Me, rangeds, DistanceMode.IgnoreAll) > 30)
                {
                    continue;
                }
                if (rangeds.CurrentJob() == Jobs.BlackMage || rangeds.CurrentJob() == Jobs.RedMage || rangeds.CurrentJob() == Jobs.Summoner||rangeds.CurrentJob()==Jobs.Pictomancer)
                {
                    if (Core.Resolve<MemApiSpell>().CheckActionInRangeOrLoS((uint)WARActionID.原初的勇猛, rangeds))
                    {
                        Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog($"勇猛{rangeds.Name}");
                        Core.Resolve<MemApiChatMessage>().Toast2($"勇猛{rangeds.Name}", 1, 2000);
                        SpellID = (uint)WARActionID.原初的勇猛;
                        target = rangeds;
                        return base.Check();
                    }
                }
            }
            return -404;
        }
    }
}

