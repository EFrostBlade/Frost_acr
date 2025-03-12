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
    internal class BOSS中_保留尽毁 : Frost_WAR_ISlotResolver
    {
        public override int Check()
        {

            if (BattleData.ResolverState != 5)
            {
                return -1;
            }
            if (!QT.GetQt("保留尽毁"))
            {
                return -2;
            }

            int baseCheck = CanUseGCD((uint)WARActionID.尽毁, 0);
            if (baseCheck != 0)
            {
                return baseCheck;
            }
            if (BattleData.尽毁预备剩余时间 > 3f)
            {
                return -5;
            }
            SpellID = (uint)WARActionID.尽毁;
            target = BattleData.当前目标;
            return base.Check();
        }
    }
}

