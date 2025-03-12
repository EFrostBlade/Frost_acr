using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.CombatRoutine.Module.Target;
using AEAssist.Extension;
using AEAssist.Helper;
using AEAssist.MemoryApi;
using Dalamud.Game.ClientState.Objects.Types;
using Frost.Frost_WAR.Frost_WAR_Data;


namespace Frost.Frost_WAR.Frost_WAR_SlotResolvers
{
    internal class 泄资源_飞斧 : Frost_WAR_ISlotResolver
    {
        public override int Check()
        {


            if (BattleData.ResolverState != 1)
            {
                return -1;
            }
            if (QT.GetQt("禁用飞斧"))
            {
                return -2;
            }
            int baseCheck = CanUseGCD((uint)WARActionID.飞斧, 0, false, false);
            if (baseCheck != 0)
            {
                return baseCheck;
            }

            if (BattleData.当前目标 == null || BattleData.目标距离 > 20)
            {
                foreach (var keyValuePair in TargetMgr.Instance.EnemysIn20)
                {
                    IBattleChara battleChara = keyValuePair.Value;
                    if (Core.Resolve<MemApiSpell>().CheckActionInRangeOrLoS((uint)WARActionID.飞斧, battleChara))
                    {
                        SpellID = (uint)WARActionID.飞斧;
                        target = battleChara;
                        return base.Check();
                    }
                }
            }
            if (Core.Resolve<MemApiSpell>().CheckActionInRangeOrLoS((uint)WARActionID.飞斧, BattleData.当前目标))
            {
                SpellID = (uint)WARActionID.飞斧;
                target = BattleData.当前目标;
                return base.Check();
            }

            return -404;
        }
    }
}

