using AEAssist.CombatRoutine;
using Frost.Frost_PLD.Frost_PLD_Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frost.Frost_PLD.Frost_PLD_SlotResolvers
{
    internal class BOSS中_悔罪 : Frost_PLD_ISlotResolver
    {
        public override int Check()
        {

            if (BattleData.ResolverState != 5)
            {
                return -1;
            }
            if (BattleData.悔罪预备剩余时间 == 0)
            {
                return -3;
            }
            int baseCheck = CanUseGCD((uint)PLDActionID.悔罪, false);
            if (baseCheck != 0)
            {
                return baseCheck;
            }
            target = GetAOETarget();
            if (target == null)
            {
                return -404;
            }
            SpellID = (uint)PLDActionID.悔罪;
            return base.Check();
        }
    }
}
