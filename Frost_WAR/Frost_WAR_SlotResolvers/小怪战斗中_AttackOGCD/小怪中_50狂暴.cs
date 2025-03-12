﻿using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Extension;
using AEAssist.Helper;
using AEAssist.JobApi;
using AEAssist.MemoryApi;
using Dalamud.Game.ClientState.Objects.Types;
using Frost.Frost_WAR.Frost_WAR_Data;

namespace Frost.Frost_WAR.Frost_WAR_SlotResolvers
{
    internal class 小怪中_50狂暴 : Frost_WAR_ISlotResolver
    {
        public override int Check()
        {
            if (BattleData.ResolverState != 3)
            {
                return -1;
            }
            if (Core.Me.Level >= 50)
            {
                return -2;
            }
            int baseCheck = CanUseAttactOGCD((uint)WARActionID.狂暴, true, false, 0);
            if (baseCheck != 0)
            {
                return baseCheck;
            }
            if (Core.Resolve<JobApi_Warrior>().BeastGauge < 30)
            {
                return -4;
            }

            double cooldown = SpellHelper.GetSpell((uint)WARActionID.狂暴).Cooldown.TotalSeconds;

            if (cooldown > Setting.技能提前时间)
            {
                return -400;
            }

            SpellID = (uint)WARActionID.狂暴;
            target = Core.Me;
            return base.Check();
        }
    }
}

