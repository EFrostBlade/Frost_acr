using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Extension;
using AEAssist.Helper;
using Dalamud.Game.ClientState.Objects.Types;
using Frost.Frost_PLD.Frost_PLD_Data;

namespace Frost.Frost_PLD.Frost_PLD_SlotResolvers
{
    internal class 技能使用_钢铁信念 : Frost_PLD_ISlotResolver
    {
        public override int Check()
        {
            if (!SC.GetSC("钢铁信念"))
            {
                return -1;
            }

            int baseCheck = CanUseDefenceOGCD((uint)PLDActionID.钢铁信念, false, SC.GetSCForceInsert("钢铁信念"));
            if (baseCheck == -600)
            {
                return -600;
            }
            if (baseCheck != 0)
            {
                return baseCheck;
            }
            double cooldown = SpellHelper.GetSpell((uint)PLDActionID.钢铁信念).Cooldown.TotalSeconds;
            if (cooldown > Setting.技能提前时间)
            {
                return -400;
            }

            Frost_PLD_RotationEntry.Frost_PLD_ArcUI.AddLog("钢铁信念 技能使用");
            SpellID = (uint)PLDActionID.钢铁信念;
            target = SC.GetSCTarget("钢铁信念");
            if (target == null)
            {
                return -404;
            }
            return base.Check();
        }
    }
}

