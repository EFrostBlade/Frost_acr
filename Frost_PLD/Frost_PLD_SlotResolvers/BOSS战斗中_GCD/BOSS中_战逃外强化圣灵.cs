using AEAssist.CombatRoutine.Module.Target;
using AEAssist.MemoryApi;
using AEAssist;
using Dalamud.Game.ClientState.Objects.Types;
using Frost.Frost_PLD.Frost_PLD_Data;
using Frost.Frost_WAR.Frost_WAR_Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frost.Frost_PLD.Frost_PLD_SlotResolvers
{
    internal class BOSS中_战逃外强化圣灵 : Frost_PLD_ISlotResolver
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
            if (BattleData.神圣魔法效果提高剩余时间 == 0)
            {
                return -3;
            }
            int baseCheck = CanUseGCD((uint)PLDActionID.圣灵, false);
            if (baseCheck != 0)
            {
                return baseCheck;
            }
            if (BattleData.当前目标 == null || BattleData.目标距离 > 25)
            {
                foreach (var keyValuePair in TargetMgr.Instance.EnemysIn25)
                {
                    IBattleChara battleChara = keyValuePair.Value;
                    if (Core.Resolve<MemApiSpell>().CheckActionInRangeOrLoS((uint)PLDActionID.圣灵, battleChara))
                    {
                        SpellID = (uint)PLDActionID.圣灵;
                        target = battleChara;
                        return base.Check();
                    }
                }
            }
            if (Core.Resolve<MemApiSpell>().CheckActionInRangeOrLoS((uint)PLDActionID.圣灵, BattleData.当前目标))
            {
                SpellID = (uint)PLDActionID.圣灵;
                target = BattleData.当前目标;
                return base.Check();
            }
            return -404;
        }
    }
}
