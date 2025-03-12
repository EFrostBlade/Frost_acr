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
    internal class BOSS战斗中_猛攻 : Frost_WAR_ISlotResolver
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
            if (SpellHelper.GetSpell((uint)WARActionID.猛攻).Charges > 2.9f)
            {
                return -2;
            }
            if (QT.GetQt("保留尽毁"))
            {
                return -3;
            }
            if(BattleData.原初的觉悟剩余时间==0)
            {
                return -4;
            }
            if (BattleData.战斗开始分钟数 % 2 == 1&&BattleData.爆发药剩余时间==0&& !QT.GetQt("打完猛攻"))
            {
                return -5;
            }
            int baseCheck = CanUseAttactOGCD((uint)WARActionID.猛攻, false, false, 1);
            if (baseCheck != 0)
            {
                return baseCheck;
            }
            if (!QT.GetQt("打完猛攻"))
            {
                if (SpellHelper.GetSpell((uint)WARActionID.猛攻).Charges < (float)Setting.保留猛攻层数 + 1f)
                {
                    return -400;
                }
            }
            else
            {
                if (SpellHelper.GetSpell((uint)WARActionID.猛攻).Charges < 1f)
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


            SpellID = (uint)WARActionID.猛攻;
            target = BattleData.当前目标;
            return base.Check();
        }
    }
}

