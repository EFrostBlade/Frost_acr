using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Extension;
using AEAssist.Helper;
using Dalamud.Game.ClientState.Objects.Types;
using Frost.Frost_PLD.Frost_PLD_Data;

namespace Frost.Frost_PLD.Frost_PLD_SlotResolvers
{
    internal class 前置_关盾 : Frost_PLD_ISlotResolver
    {
        public override int Check()
        {
            if (QT.GetQt("盾姿")||!Setting.启用qt控制盾姿)
            {
                return -1;
            }
            if (SpellHelper.GetSpell((uint)PLDActionID.解除钢铁信念).RecentlyUsed())
            {
                return -2;
            }
            if (!BattleData.是否开盾)
            {
                return -3;
            }
            if (SpellHelper.GetSpell((uint)PLDActionID.解除钢铁信念).RecentlyUsed()|| SpellHelper.GetSpell((uint)PLDActionID.钢铁信念).RecentlyUsed())
            {
                return -4;
            }
            int baseCheck = CanUseDefenceOGCD((uint)PLDActionID.解除钢铁信念);
            if(baseCheck!= 0)
            {
                return baseCheck;
            }
            double cooldown = SpellHelper.GetSpell((uint)PLDActionID.解除钢铁信念).Cooldown.TotalSeconds;
            if (cooldown > Setting.技能提前时间)
            {
                return -400;
            }
            Frost_PLD_RotationEntry.Frost_PLD_ArcUI.AddLog("关盾");
            SpellID = (uint)PLDActionID.解除钢铁信念;
            target = Core.Me;
            return base.Check();
        }
    }
}

