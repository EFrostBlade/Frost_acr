using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Extension;
using AEAssist.Helper;
using Dalamud.Game.ClientState.Objects.Types;
using Frost.Frost_PLD.Frost_PLD_Data;

namespace Frost.Frost_PLD.Frost_PLD_SlotResolvers
{
    internal class 技能使用_极致防御 : Frost_PLD_ISlotResolver
    {
        public override int Check()
        {
            if (!SC.GetSC("极致防御"))
            {
                return -1;
            }

            if (Core.Me.Level >= 92)
            {

                int baseCheck = CanUseDefenceOGCD((uint)PLDActionID.极致防御, false, SC.GetSCForceInsert("极致防御"));
                if (baseCheck == -600)
                {
                    return -600;
                }
                if (baseCheck != 0)
                {
                    return baseCheck;
                }
                double cooldown = SpellHelper.GetSpell((uint)PLDActionID.极致防御).Cooldown.TotalSeconds;
                if (cooldown > Setting.技能提前时间)
                {
                    return -400;
                }

                Frost_PLD_RotationEntry.Frost_PLD_ArcUI.AddLog("极致防御 技能使用");
                SpellID = (uint)PLDActionID.极致防御;
                target = SC.GetSCTarget("极致防御");
                if (target == null)
                {
                    return -404;
                }
                return base.Check();
            }
            else
            {
                int baseCheck = CanUseDefenceOGCD((uint)PLDActionID.预警, false, SC.GetSCForceInsert("极致防御"));
                if (baseCheck == -600)
                {
                    return -600;
                }
                if (baseCheck != 0)
                {
                    return baseCheck;
                }
                double cooldown = SpellHelper.GetSpell((uint)PLDActionID.预警).Cooldown.TotalSeconds;
                if (cooldown > Setting.技能提前时间)
                {
                    return -400;
                }
                Frost_PLD_RotationEntry.Frost_PLD_ArcUI.AddLog("预警 技能使用");
                SpellID = (uint)PLDActionID.预警;
                target = SC.GetSCTarget("极致防御");
                if (target == null)
                {
                    return -404;
                }
                return base.Check();
            }
        }
    }
}

