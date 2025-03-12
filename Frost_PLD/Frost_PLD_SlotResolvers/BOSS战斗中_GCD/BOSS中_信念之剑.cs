using Frost.Frost_PLD.Frost_PLD_Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frost.Frost_PLD.Frost_PLD_SlotResolvers
{
    internal class BOSS中_信念之剑 : Frost_PLD_ISlotResolver
    {
        public override int Check()
        {

            if (BattleData.ResolverState != 5)
            {
                return -1;
            }
            //if (BattleData.上次魔法连击技能 != (uint)PLDActionID.悔罪)
            //{
            //    return -3;
            //}
            //if (BattleData.魔法连击剩余时间 == 0)
            //{
            //    return -5;
            //}
            int baseCheck = CanUseGCD((uint)PLDActionID.信念之剑);
            if (baseCheck != 0)
            {
                return baseCheck;
            }
            target = GetAOETarget();
            if (target == null)
            {
                return -404;
            }
            SpellID = (uint)PLDActionID.信念之剑;
            return base.Check();
        }
    }
}
