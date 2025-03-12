using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.CombatRoutine.Module.Target;
using AEAssist.Extension;
using AEAssist.Helper;
using AEAssist.MemoryApi;
using Dalamud.Game.ClientState.Objects.Types;
using Frost.Frost_WAR.Frost_WAR_Data;


namespace Frost.Frost_WAR.Frost_WAR_SlotResolvers
{
    internal class 小怪结_秘银暴风 : Frost_WAR_ISlotResolver
    {
        public override int Check()
        {

            if (BattleData.ResolverState != 4)
            {
                return -1;
            }
            if (BattleData.上次连击技能 != (uint)WARActionID.超压斧)
            {
                return -3;
            }
            int baseCheck = CanUseGCD((uint)WARActionID.秘银暴风, 0,false,true);
            if (baseCheck != 0)
            {
                return baseCheck;
            }
            SpellID = (uint)WARActionID.秘银暴风;
            target = Core.Me;
            return base.Check();
        }
    }
}

