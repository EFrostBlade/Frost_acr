using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Extension;
using AEAssist.Helper;
using AEAssist.MemoryApi;
using Dalamud.Game.ClientState.Objects.Types;
using Frost.Frost_WAR.Frost_WAR_Data;
using Frost.Frost_WAR.Frost_WAR_Setting;

namespace Frost.Frost_WAR.Frost_WAR_SlotResolvers
{
    internal class 泄资源_猛攻 : Frost_WAR_ISlotResolver
    {
        public override int Check()
        {
            if (Setting.禁用所有位移技能)
            {
                return -1919;
            }
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
            int baseCheck = CanUseAttactOGCD((uint)WARActionID.猛攻, false);
            if (baseCheck != 0)
            {
                return baseCheck;
            }
            if (SpellHelper.GetSpell((uint)WARActionID.猛攻).Charges < 1f)
            {
                return -400;
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


            SpellID = (uint)WARActionID.猛攻;
            target = BattleData.当前目标;
            return base.Check();
        }
    }
}

