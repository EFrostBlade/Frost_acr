using Frost.Frost_PLD.Frost_PLD_Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frost.Frost_PLD.Frost_PLD_SlotResolvers
{
    internal class BOSS中_沥血剑 : Frost_PLD_ISlotResolver
    {
        public override int Check()
        {

            if (BattleData.ResolverState != 5)
            {
                return -1;
            }
            if (BattleData.沥血剑预备剩余时间==0)
            {
                return -2;
            }
            int baseCheck = CanUseGCD((uint)PLDActionID.沥血剑);
            if (baseCheck != 0)
            {
                return baseCheck;
            }
            SpellID = (uint)PLDActionID.沥血剑;
            target = BattleData.当前目标;
            return base.Check();
        }
    }
}
