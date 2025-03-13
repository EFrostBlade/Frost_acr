using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.CombatRoutine.Module.Target;
using AEAssist.Extension;
using AEAssist.Helper;
using AEAssist.MemoryApi;
using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Frost.Frost_PLD.Frost_PLD_Data;

namespace Frost.Frost_PLD.Frost_PLD_SlotResolvers
{
    internal class 技能使用_下踢 : Frost_PLD_ISlotResolver
    {
        public override int Check()
        {
            if (!SC.GetSC("下踢"))
            {
                return -1;
            }
            int baseCheck = CanUseAttactOGCD((uint)PLDActionID.下踢, false, false, SC.GetSCForceInsert("下踢"));
            if (baseCheck == -600)
            {
                return -600;
            }
            if (baseCheck != 0)
            {
                return baseCheck;
            }
            double cooldown = SpellHelper.GetSpell((uint)PLDActionID.下踢).Cooldown.TotalSeconds;
            if (cooldown > Setting.技能提前时间)
            {
                return -400;
            }
            SpellID = (uint)PLDActionID.下踢;
            target = SC.GetSCTarget("下踢");
            if (target == null)
            {
                return -404;
            }
            return base.Check();
        }
    }
}

