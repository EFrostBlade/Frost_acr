using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Extension;
using AEAssist.Helper;
using Dalamud.Game.ClientState.Objects.Types;
using Frost.Frost_PLD.Frost_PLD_Data;

namespace Frost.Frost_PLD.Frost_PLD_SlotResolvers
{
    internal class BOSS战斗中_战逃外偿赎剑 : Frost_PLD_ISlotResolver
    {
        public override int Check()
        {
            if (BattleData.ResolverState != 5)
            {
                return -1;
            }
            if (BattleData.战逃反应剩余时间 > 0)
            {
                return -2;
            }
            if (Core.Me.Level < 86)
            {
                return -3;
            }
            if (SpellHelper.GetSpell((uint)PLDActionID.战逃反应).Cooldown.TotalSeconds < 10)
            {
                return -4;
            }
            double cooldown = SpellHelper.GetSpell((uint)PLDActionID.偿赎剑).Cooldown.TotalSeconds;

            if (cooldown > Setting.技能提前时间)
            {
                return -400;
            }
            int baseCheck = CanUseAttactOGCD((uint)PLDActionID.偿赎剑, true, false);
            if (baseCheck != 0)
            {
                return baseCheck;
            }


            target = GetAOETarget((float)SettingMgr.GetSetting<GeneralSettings>().AttackRange);
            if (target == null)
            {
                return -404;
            }
            SpellID = (uint)PLDActionID.偿赎剑;
            return base.Check();
        }
    }
}

