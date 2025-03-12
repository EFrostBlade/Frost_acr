﻿using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Extension;
using AEAssist.Helper;
using AEAssist.JobApi;
using Dalamud.Game.ClientState.Objects.Types;
using Frost.Frost_WAR.Frost_WAR_Data;

namespace Frost.Frost_WAR.Frost_WAR_SlotResolvers
{
    internal class 小怪结_满层战嚎 : Frost_WAR_ISlotResolver
    {
        public override int Check()
        {
            if (BattleData.ResolverState != 4)
            {
                return -1;
            }
            int baseCheck = CanUseAttactOGCD((uint)WARActionID.战嚎, false);
            if (baseCheck != 0)
            {
                return baseCheck;
            }
            if (BattleData.原初的混沌剩余时间 > 0)
            {
                return -2;
            }
            if (BattleData.狂暴剩余时间 > 0)
            {
                return -3;
            }
            if (Core.Resolve<JobApi_Warrior>().BeastGauge > 50)
            {
                return -5;
            }
            if (BattleData.原初的解放层数 > 0)
            {
                return -6;
            }
            if (SpellHelper.GetSpell((uint)WARActionID.战嚎).Charges < 1.9f)
            {
                return -400;
            }

            SpellID = (uint)WARActionID.战嚎;
            target = Core.Me;
            return base.Check();
        }
    }
}

