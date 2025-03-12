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
    internal class 拉怪_蛮荒崩裂 : Frost_WAR_ISlotResolver
    {
        public override int Check()
        {
            if (Setting.禁用所有位移技能)
            {
                return -1919;
            }
            if (BattleData.ResolverState != 2)
            {
                return -1;
            }
            if (BattleData.蛮荒崩裂预备剩余时间 >4)
            {
                return -2;
            }
            int baseCheck = CanUseGCD((uint)WARActionID.蛮荒崩裂, 0, false);
            if (baseCheck != 0)
            {
                return baseCheck;
            }
            SpellID = (uint)WARActionID.蛮荒崩裂;
            target = BattleData.当前目标;
            return base.Check();
        }
    }
}

