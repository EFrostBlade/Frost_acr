using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.CombatRoutine.Module.Target;
using AEAssist.Extension;
using AEAssist.Helper;
using AEAssist.JobApi;
using AEAssist.MemoryApi;
using Dalamud.Game.ClientState.Objects.Types;
using Frost.Frost_WAR.Frost_WAR_Data;


namespace Frost.Frost_WAR.Frost_WAR_SlotResolvers
{
    internal class 小怪结_钢铁旋风 : Frost_WAR_ISlotResolver
    {
        public override int Check()
        {

            if (BattleData.ResolverState != 4)
            {
                return -1;
            }
            if(Core.Resolve<JobApi_Warrior>().BeastGauge < 80)
            {
                return -2;
            }


            int baseCheck = CanUseGCD((uint)WARActionID.地毁人亡, 1, false, true);
            if (baseCheck != 0)
            {
                int baseCheck2 = CanUseGCD((uint)WARActionID.钢铁旋风, 1, false, true);
                if (baseCheck2 != 0)
                {
                    return baseCheck2;
                }
                SpellID = (uint)WARActionID.钢铁旋风;
                target = Core.Me;
                return base.Check();
            }
            SpellID = (uint)WARActionID.地毁人亡;
            target = Core.Me;
            return base.Check();
        }
    }
}

