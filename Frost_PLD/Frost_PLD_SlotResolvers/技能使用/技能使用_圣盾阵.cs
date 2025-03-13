using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Extension;
using AEAssist.Helper;
using Dalamud.Game.ClientState.Objects.Types;
using Frost.Frost_PLD.Frost_PLD_Data;

namespace Frost.Frost_PLD.Frost_PLD_SlotResolvers
{
    internal class 技能使用_圣盾阵 : Frost_PLD_ISlotResolver
    {
        public override int Check()
        {
            if (!SC.GetSC("圣盾阵"))
            {
                return -1;
            }
            if (Core.Me.Level >= 82)
            {
                int baseCheck = CanUseDefenceOGCD((uint)PLDActionID.圣盾阵, false, SC.GetSCForceInsert("圣盾阵"));
                if (baseCheck == -600)
                {
                    return -600;
                }
                if (baseCheck != 0)
                {
                    return baseCheck;
                }
                double cooldown = SpellHelper.GetSpell((uint)PLDActionID.圣盾阵).Cooldown.TotalSeconds;
                if (cooldown > Setting.技能提前时间)
                {
                    return -400;
                }

                Frost_PLD_RotationEntry.Frost_PLD_ArcUI.AddLog("圣盾阵 技能使用");
                SpellID = (uint)PLDActionID.圣盾阵;
                target = SC.GetSCTarget("圣盾阵");
                if (target == null)
                {
                    return -404;
                }
                return base.Check();
            }
            else
            {
                int baseCheck = CanUseDefenceOGCD((uint)PLDActionID.盾阵, false, SC.GetSCForceInsert("圣盾阵"));
                if (baseCheck == -600)
                {
                    return -600;
                }
                if (baseCheck != 0)
                {
                    return baseCheck;
                }
                double cooldown = SpellHelper.GetSpell((uint)PLDActionID.盾阵).Cooldown.TotalSeconds;
                if (cooldown > Setting.技能提前时间)
                {
                    return -400;
                }
                Frost_PLD_RotationEntry.Frost_PLD_ArcUI.AddLog("盾阵 技能使用");
                SpellID = (uint)PLDActionID.盾阵;
                target = SC.GetSCTarget("圣盾阵");
                if (target == null)
                {
                    return -404;
                }
                return base.Check();
            }
        }
    }
}

