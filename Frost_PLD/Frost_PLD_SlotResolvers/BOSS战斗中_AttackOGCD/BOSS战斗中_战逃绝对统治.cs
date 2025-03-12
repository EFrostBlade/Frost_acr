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
    internal class BOSS战斗中_战逃绝对统治 : Frost_PLD_ISlotResolver
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
            if (Core.Me.Level < 96)
            {
                return -3;
            }
            int baseCheck = CanUseAttactOGCD((uint)PLDActionID.绝对统治, false, false);
            if (baseCheck != 0)
            {
                return baseCheck;
            }

            double cooldown = SpellHelper.GetSpell((uint)PLDActionID.绝对统治).Cooldown.TotalSeconds;

            if (cooldown > Setting.技能提前时间)
            {
                return -400;
            }

            target = GetAOETarget();
            if (target == null)
            {
                return -404;
            }
            SpellID = (uint)PLDActionID.绝对统治;
            return base.Check();
        }
    }
}

