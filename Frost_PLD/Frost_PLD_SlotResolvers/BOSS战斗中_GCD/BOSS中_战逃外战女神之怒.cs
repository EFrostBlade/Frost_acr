using AEAssist;
using Frost.Frost_PLD.Frost_PLD_Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frost.Frost_PLD.Frost_PLD_SlotResolvers
{
    internal class BOSS中_战逃外战女神之怒 : Frost_PLD_ISlotResolver
    {
        public override int Check()
        {

            if (BattleData.ResolverState != 5)
            {
                return -1;
            }
            if(Core.Me.Level >= 60)
            {
                return -6;
            }
            if (BattleData.战逃反应剩余时间 > 0)
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
            int baseCheck = CanUseGCD((uint)PLDActionID.战女神之怒);
            if (baseCheck != 0)
            {
                return baseCheck;
            }
            SpellID = (uint)PLDActionID.战女神之怒;
            target = BattleData.当前目标;
            return base.Check();
        }
    }
}
