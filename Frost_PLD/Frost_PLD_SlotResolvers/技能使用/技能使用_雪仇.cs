using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.CombatRoutine.Module.Target;
using AEAssist.Extension;
using AEAssist.Helper;
using Dalamud.Game.ClientState.Objects.Types;
using Frost.Frost_PLD.Frost_PLD_Data;

namespace Frost.Frost_PLD.Frost_PLD_SlotResolvers
{
    internal class 技能使用_雪仇 : Frost_PLD_ISlotResolver
    {
        public override int Check()
        {
            if (!SC.GetSC("雪仇"))
            {
                return -1;
            }
            if (BattleData.目标距离 > 5 && !BattleData.是否可打aoe)
            {
                return -2;
            }

            int baseCheck = CanUseDefenceOGCD((uint)PLDActionID.雪仇, false, SC.GetSCForceInsert("雪仇"));
            if (baseCheck == -600)
            {
                return -600;
            }
            if (baseCheck != 0)
            {
                return baseCheck;
            }
            double cooldown = SpellHelper.GetSpell((uint)PLDActionID.雪仇).Cooldown.TotalSeconds;
            if (cooldown > Setting.技能提前时间)
            {
                return -400;
            }

            Frost_PLD_RotationEntry.Frost_PLD_ArcUI.AddLog("雪仇 技能使用");
            SpellID = (uint)PLDActionID.雪仇;
            target = SC.GetSCTarget("雪仇");
            if (target == null)
            {
                return -404;
            }
            return base.Check();
        }
    }
}

