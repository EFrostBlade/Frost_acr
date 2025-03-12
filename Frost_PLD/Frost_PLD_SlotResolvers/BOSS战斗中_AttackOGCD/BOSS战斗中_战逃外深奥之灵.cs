using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Extension;
using AEAssist.Helper;
using Dalamud.Game.ClientState.Objects.Types;
using Frost.Frost_PLD.Frost_PLD_Data;

namespace Frost.Frost_PLD.Frost_PLD_SlotResolvers
{
    internal class BOSS战斗中_战逃外深奥之灵 : Frost_PLD_ISlotResolver
    {
        public override int Check()
        {
            if (BattleData.ResolverState != 5)
            {
                return -1;
            }
            if (BattleData.战逃反应剩余时间 > 0)
            {
                return -2;
            }
            if (Core.Me.Level >= 86)
            {
                return -3;
            }
            if (SpellHelper.GetSpell((uint)PLDActionID.战逃反应).Cooldown.TotalSeconds < 10)
            {
                return -4;
            }
            int baseCheck = CanUseAttactOGCD((uint)PLDActionID.深奥之灵, true, false);
            if (baseCheck != 0)
            {
                return baseCheck;
            }

            double cooldown = SpellHelper.GetSpell((uint)PLDActionID.深奥之灵).Cooldown.TotalSeconds;

            if (cooldown > Setting.技能提前时间)
            {
                return -400;
            }

            SpellID = (uint)PLDActionID.深奥之灵;
            target = BattleData.当前目标;
            return base.Check();
        }
    }
}

