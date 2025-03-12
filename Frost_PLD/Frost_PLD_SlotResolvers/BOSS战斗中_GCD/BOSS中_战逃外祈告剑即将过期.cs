using Frost.Frost_PLD.Frost_PLD_Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frost.Frost_PLD.Frost_PLD_SlotResolvers
{
    internal class BOSS中_战逃外祈告剑即将过期 : Frost_PLD_ISlotResolver
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
            if (BattleData.祈告剑预备剩余时间==0)
            {
                return -3;
            }
            if (BattleData.神圣魔法效果提高剩余时间 > 0 && BattleData.神圣魔法效果提高剩余时间 < 6)
            {
                if (BattleData.祈告剑预备剩余时间 > 6)
                {
                    return -4;
                }
            }
            if (BattleData.祈告剑预备剩余时间 > 3)
            {
                return -5;
            }
            int baseCheck = CanUseGCD((uint)PLDActionID.祈告剑);
            if (baseCheck != 0)
            {
                return baseCheck;
            }
            SpellID = (uint)PLDActionID.祈告剑;
            target = BattleData.当前目标;
            return base.Check();
        }
    }
}
