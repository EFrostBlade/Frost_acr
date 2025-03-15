using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Extension;
using AEAssist.Helper;
using Dalamud.Game.ClientState.Objects.Types;
using Frost.Frost_PLD.Frost_PLD_Data;

namespace Frost.Frost_PLD.Frost_PLD_SlotResolvers
{
    internal class 技能使用_战逃反应 : Frost_PLD_ISlotResolver
    {
        public override int Check()
        {
            if (!SC.GetSC("战逃反应"))
            {
                return -1;
            }

            int baseCheck = CanUseDefenceOGCD((uint)PLDActionID.战逃反应, false, SC.GetSCForceInsert("战逃反应"));
            if (baseCheck == -600)
            {
                return -600;
            }
            if (baseCheck != 0)
            {
                return baseCheck;
            }
            double cooldown = SpellHelper.GetSpell((uint)PLDActionID.战逃反应).Cooldown.TotalSeconds;
            if (cooldown > Setting.技能提前时间)
            {
                return -400;
            }

            Frost_PLD_RotationEntry.Frost_PLD_ArcUI.AddLog("战逃反应 技能使用");
            SpellID = (uint)PLDActionID.战逃反应;
            target = SC.GetSCTarget("战逃反应");
            if (target == null)
            {
                return -404;
            }
            return base.Check();
        }
    }
}

