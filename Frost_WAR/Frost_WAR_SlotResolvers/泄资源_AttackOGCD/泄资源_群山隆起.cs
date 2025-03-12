using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Extension;
using AEAssist.Helper;
using Dalamud.Game.ClientState.Objects.Types;
using Frost.Frost_WAR.Frost_WAR_Data;

namespace Frost.Frost_WAR.Frost_WAR_SlotResolvers
{
    internal class 泄资源_群山隆起 : Frost_WAR_ISlotResolver
    {
        public override int Check()
        {
            if (BattleData.ResolverState != 1)
            {
                return -1;
            }
            if (!QT.GetQt("没红斩也泄"))
            {
                if (BattleData.战场风暴剩余时间 < 1f)
                {
                    return -2;
                }
            }
            int baseCheck = CanUseAttactOGCD((uint)WARActionID.群山隆起, false, true);
            if (baseCheck != 0)
            {
                return baseCheck;
            }
            double cooldown = SpellHelper.GetSpell((uint)WARActionID.群山隆起).Cooldown.TotalSeconds;

            if (cooldown > Setting.技能提前时间)
            {
                return -400;
            }

            SpellID = (uint)WARActionID.群山隆起;
            target = Core.Me;
            return base.Check();
        }
    }
}

