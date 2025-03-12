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
    internal class 小怪中_蛮荒崩裂 : Frost_WAR_ISlotResolver
    {
        public override int Check()
        {
            if (Setting.禁用所有位移技能)
            {
                return -1919;
            }

            if (BattleData.ResolverState != 3)
            {
                return -1;
            }

            int baseCheck = CanUseGCD((uint)WARActionID.蛮荒崩裂, 1, false);
            if (baseCheck != 0)
            {
                return baseCheck;
            }
            if (!QT.GetQt("允许突进"))
            {
                if (!BattleData.目标是否在近战范围内)
                {
                    return -3;
                }
                if(Core.Resolve<MemApiMove>().IsMoving())
                {
                    return -4;
                }
            }

            SpellID = (uint)WARActionID.蛮荒崩裂;
            target = BattleData.当前目标;
            return base.Check();
        }
    }
}

