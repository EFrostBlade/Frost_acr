using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Extension;
using AEAssist.Helper;
using Dalamud.Game.ClientState.Objects.Types;
using Frost.Frost_WAR.Frost_WAR_Data;

namespace Frost.Frost_WAR.Frost_WAR_SlotResolvers
{
    internal class 前置_关盾 : Frost_WAR_ISlotResolver
    {
        public override int Check()
        {
            if (QT.GetQt("盾姿")||!Setting.启用qt控制盾姿)
            {
                return -1;
            }
            if (SpellHelper.GetSpell((uint)WARActionID.解除守护).RecentlyUsed())
            {
                return -2;
            }
            if (!BattleData.是否开盾)
            {
                return -3;
            }
            if (SpellHelper.GetSpell((uint)WARActionID.解除守护).RecentlyUsed()|| SpellHelper.GetSpell((uint)WARActionID.守护).RecentlyUsed())
            {
                return -4;
            }
            int baseCheck = CanUseDefenceOGCD((uint)WARActionID.解除守护);
            if(baseCheck!= 0)
            {
                return baseCheck;
            }
            double cooldown = SpellHelper.GetSpell((uint)WARActionID.解除守护).Cooldown.TotalSeconds;
            if (cooldown > Setting.技能提前时间)
            {
                return -400;
            }
            Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("关盾");
            SpellID = (uint)WARActionID.解除守护;
            target = Core.Me;
            return base.Check();
        }
    }
}

