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
    internal class 小怪结_暴风碎 : Frost_WAR_ISlotResolver
    {
        public override int Check()
        {
            if (BattleData.ResolverState != 4)
            {
                return -1;
            }
            if (BattleData.战场风暴剩余时间 > 8)
            {
                return -2;
            }
            if(BattleData.上次连击技能 != (uint)WARActionID.凶残裂)
            {
                return -3;
            }
            int baseCheck = CanUseGCD((uint)WARActionID.暴风碎, 0);
            if (baseCheck != 0)
            {
                return baseCheck;
            }
            SpellID = (uint)WARActionID.暴风碎;
            target = BattleData.当前目标;
            return base.Check();
        }
    }
}

