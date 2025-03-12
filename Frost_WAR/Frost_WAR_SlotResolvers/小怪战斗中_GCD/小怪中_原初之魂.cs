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
    internal class 小怪中_原初之魂 : Frost_WAR_ISlotResolver
    {
        public override int Check()
        {

            if (BattleData.ResolverState != 3)
            {
                return -1;
            }

            int baseCheck = CanUseGCD((uint)WARActionID.裂石飞环, 1);
            if (baseCheck != 0)
            {
                int baseCheck2 = CanUseGCD((uint)WARActionID.原初之魂, 1);
                if (baseCheck2 != 0)
                {
                    return baseCheck2;
                }
                SpellID = (uint)WARActionID.原初之魂;
                target = BattleData.当前目标;
                return base.Check();
            }
            SpellID = (uint)WARActionID.裂石飞环;
            target = BattleData.当前目标;
            return base.Check();
        }
    }
}

