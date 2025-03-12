using AEAssist;
using Frost.Frost_PLD.Frost_PLD_Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frost.Frost_PLD.Frost_PLD_SlotResolvers
{
    internal class BOSS中_战逃王权剑 : Frost_PLD_ISlotResolver
    {
        public override int Check()
        {

            if (BattleData.ResolverState != 5)
            {
                return -1;
            }
            if (Core.Me.Level < 60)
            {
                return -6;
            }
            if (BattleData.战逃反应剩余时间 == 0)
            {
                return -2;
            }
            if (BattleData.上次连击技能 != (uint)PLDActionID.暴乱剑)
            {
                return -4;
            }
            if (BattleData.连击剩余时间 == 0)
            {
                return -5;
            }
            if (BattleData.神圣魔法效果提高剩余时间 > 0
                || BattleData.赎罪剑预备剩余时间 > 0
                || BattleData.祈告剑预备剩余时间 > 0
                || BattleData.葬送剑预备剩余时间 > 0)
            {
                return -6;
            }
            int baseCheck = CanUseGCD((uint)PLDActionID.王权剑);
            if (baseCheck != 0)
            {
                return baseCheck;
            }
            SpellID = (uint)PLDActionID.王权剑;
            target = BattleData.当前目标;
            return base.Check();
        }
    }
}
