using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Extension;
using AEAssist.Helper;
using AEAssist.MemoryApi;
using Dalamud.Game.ClientState.Objects.Types;
using Frost.Frost_PLD.Frost_PLD_Data;
using Frost.Frost_PLD.Frost_PLD_Setting;

namespace Frost.Frost_PLD.Frost_PLD_SlotResolvers
{
    internal class BOSS战斗中_战逃调停 : Frost_PLD_ISlotResolver
    {
        public override int Check()
        {
            if (Setting.禁用所有位移技能)
            {
                return -1919;
            }
            if (BattleData.ResolverState != 5)
            {
                return -1;
            } 
            if (BattleData.战逃反应剩余时间 == 0)
            {
                return -3;
            }
            int baseCheck = CanUseAttactOGCD((uint)PLDActionID.调停, false, false);
            if (baseCheck != 0)
            {
                return baseCheck;
            }
            if (!QT.GetQt("打完调停"))
            {
                if (SpellHelper.GetSpell((uint)PLDActionID.调停).Charges < (float)Setting.保留调停层数 + 1f)
                {
                    return -400;
                }
            }
            else
            {
                if (SpellHelper.GetSpell((uint)PLDActionID.调停).Charges < 1f)
                {
                    return -400;
                }
            }
            if (!QT.GetQt("允许突进"))
            {
                if (BattleData.目标距离 > 0)
                {
                    return -3;
                }
                if (Core.Resolve<MemApiMove>().IsMoving())
                {
                    return -4;
                }
            }


            SpellID = (uint)PLDActionID.调停;
            target = BattleData.当前目标;
            return base.Check();
        }
    }
}

