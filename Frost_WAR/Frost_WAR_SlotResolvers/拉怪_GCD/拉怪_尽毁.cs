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
    internal class 拉怪_尽毁 : Frost_WAR_ISlotResolver
    {
        public override int Check()
        {
            if (BattleData.ResolverState != 2)
            {
                return -1;
            }
            if (BattleData.尽毁预备剩余时间 > 4)
            {
                return -2;
            }

            int baseCheck = CanUseGCD((uint)WARActionID.尽毁);
            if (baseCheck != 0)
            {
                return baseCheck;
            }
            SpellID = (uint)WARActionID.尽毁;
            target = BattleData.当前目标;
            return base.Check();
        }
    }
}

