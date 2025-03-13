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
    internal class 技能使用_调停 : Frost_PLD_ISlotResolver
    {
        public override int Check()
        {
            if (Setting.禁用所有位移技能)
            {
                return -1919;
            }
            if (!SC.GetSC("调停"))
            {
                return -1;
            }
            int baseCheck = CanUseAttactOGCD((uint)PLDActionID.调停, false, false, SC.GetSCForceInsert("调停"));
            if (baseCheck == -600)
            {
                return -600;
            }
            if (baseCheck != 0)
            {
                return baseCheck;
            }
            float charge = SpellHelper.GetSpell((uint)PLDActionID.调停).Charges;
            double cooldown = SpellHelper.GetSpell((uint)PLDActionID.调停).Cooldown.TotalSeconds;
            if (charge < 1)
            {
                return -401;
            }
            if (charge < 1 && cooldown > Setting.技能提前时间)
            {
                return -400;
            }
            SpellID = (uint)PLDActionID.调停;
            target = SC.GetSCTarget("调停");
            if (target == null)
            {
                return -404;
            }
            return base.Check();
        }
    }
}

