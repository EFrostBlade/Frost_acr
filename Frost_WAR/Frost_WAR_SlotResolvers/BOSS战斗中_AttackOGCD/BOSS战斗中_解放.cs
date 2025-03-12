using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Extension;
using AEAssist.Helper;
using Dalamud.Game.ClientState.Objects.Types;
using Frost.Frost_WAR.Frost_WAR_Data;

namespace Frost.Frost_WAR.Frost_WAR_SlotResolvers
{
    internal class BOSS战斗中_解放 : Frost_WAR_ISlotResolver
    {
        public override int Check()
        {
            if (BattleData.ResolverState != 5)
            {
                return -1;
            }
            int baseCheck = CanUseAttactOGCD((uint)WARActionID.原初的解放, true, false, 1);
            if (baseCheck != 0)
            {
                return baseCheck;
            }
            if(BattleData.原初的混沌剩余时间>0)
            {
                return -2;
            }

            double cooldown = SpellHelper.GetSpell((uint)WARActionID.原初的解放).Cooldown.TotalSeconds;

            if (cooldown > Setting.技能提前时间)
            {
                return -400;
            }

            SpellID = (uint)WARActionID.原初的解放;
            target = Core.Me;
            return base.Check();
        }
    }
}

