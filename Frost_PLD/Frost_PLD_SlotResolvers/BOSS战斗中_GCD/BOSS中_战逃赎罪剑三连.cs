using AEAssist.MemoryApi;
using AEAssist;
using Frost.Frost_PLD.Frost_PLD_Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frost.Frost_PLD.Frost_PLD_SlotResolvers
{
    internal class BOSS中_战逃赎罪剑三连 : Frost_PLD_ISlotResolver
    {
        public override int Check()
        {

            if (BattleData.ResolverState != 5)
            {
                return -1;
            }
            if (BattleData.战逃反应剩余时间 == 0)
            {
                return -2;
            }
            if (BattleData.赎罪剑预备剩余时间 == 0)
            {
                return -3;
            }
            if (BattleData.战逃反应剩余时间 > 8)
            {
                return -4;
            }
            if (BattleData.战逃反应剩余时间 < 6)
            {
                return -5;
            }
            int baseCheck = CanUseGCD((uint)PLDActionID.赎罪剑);
            if (baseCheck != 0)
            {
                return baseCheck;
            }
            SpellID = (uint)PLDActionID.赎罪剑;
            target = BattleData.当前目标;
            return base.Check();
        }
    }
}
