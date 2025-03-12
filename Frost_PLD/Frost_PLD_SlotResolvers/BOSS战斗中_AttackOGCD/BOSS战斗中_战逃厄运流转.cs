using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.CombatRoutine.Module.Target;
using AEAssist.Extension;
using AEAssist.Helper;
using AEAssist.MemoryApi;
using Dalamud.Game.ClientState.Objects.Types;
using Frost.Frost_PLD.Frost_PLD_Data;

namespace Frost.Frost_PLD.Frost_PLD_SlotResolvers
{
    internal class BOSS战斗中_战逃厄运流转 : Frost_PLD_ISlotResolver
    {
        public override int Check()
        {
            if (BattleData.ResolverState != 5)
            {
                return -1;
            }
            if (BattleData.战逃反应剩余时间 == 0)
            {
                return -2;
            }
            int baseCheck = CanUseAttactOGCD((uint)PLDActionID.厄运流转, false, false);
            if (baseCheck != 0)
            {
                return baseCheck;
            }

            double cooldown = SpellHelper.GetSpell((uint)PLDActionID.厄运流转).Cooldown.TotalSeconds;

            if (cooldown > Setting.技能提前时间)
            {
                return -400;
            }
            if (BattleData.目标距离 <= 5|| BattleData.是否可打aoe)
            {
                SpellID = (uint)PLDActionID.厄运流转;
                target = Core.Me;
                return base.Check();
            }

            return -404;
        }
    }
}

