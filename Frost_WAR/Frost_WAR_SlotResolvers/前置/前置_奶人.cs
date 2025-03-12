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
    internal class 前置_奶人 : Frost_WAR_ISlotResolver
    {
        public override int Check()
        {
            if (!QT.GetQt("奶人"))
            {
                return -1;
            }

            int baseCheck = CanUseDefenceOGCD((uint)WARActionID.原初的勇猛, true);
            if (baseCheck != 0)
            {
                return baseCheck;
            }
            double cooldown = SpellHelper.GetSpell((uint)WARActionID.原初的勇猛).Cooldown.TotalSeconds;
            if (cooldown > Setting.技能提前时间)
            {
                return -400;
            }

            var sortedPartyMembers = PartyHelper.CastableAlliesWithin30
                .OrderBy(p => p.CurrentHpPercent())
                .ToList();

            foreach (IBattleChara partyer in sortedPartyMembers)
            {
                if(partyer==Core.Me)
                {
                    continue;
                }
                if (partyer.CurrentHpPercent() < 0.5f)
                {
                    if (Core.Resolve<MemApiSpell>().CheckActionInRangeOrLoS((uint)WARActionID.原初的勇猛, partyer))
                    {
                        SpellID = (uint)WARActionID.原初的勇猛;
                        target = partyer;
                        Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog($"奶了{target.Name}一口");
                        if (Setting.自动减伤使用提示)
                        {
                            Core.Resolve<MemApiChatMessage>().Toast2($"奶了{target.Name}一口", 1, 2000);

                        }
                        return base.Check();
                    }
                }
            }

            return -404;
        }
    }
}

