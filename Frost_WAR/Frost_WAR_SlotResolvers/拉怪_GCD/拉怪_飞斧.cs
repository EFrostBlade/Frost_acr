using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.CombatRoutine.Module.Target;
using AEAssist.Define;
using AEAssist.Extension;
using AEAssist.Helper;
using AEAssist.MemoryApi;
using Dalamud.Game.ClientState.Objects.Types;
using Frost.Frost_WAR.Frost_WAR_Data;


namespace Frost.Frost_WAR.Frost_WAR_SlotResolvers
{
    internal class 拉怪_飞斧 : Frost_WAR_ISlotResolver
    {
        public override int Check()
        {

            if (BattleData.ResolverState != 2)
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

            foreach (var keyValuePair in TargetMgr.Instance.EnemysIn20)
            {
                IBattleChara enemy = keyValuePair.Value;
                if (GameObjectExtension.Distance(Core.Me, enemy, DistanceMode.IgnoreAll) <= 20
                    && Core.Resolve<MemApiSpell>().CheckActionInRangeOrLoS((uint)WARActionID.飞斧, enemy))
                {
                    SpellID = (uint)WARActionID.飞斧;
                    target = enemy;
                    return base.Check();
                }
            }

            return -404;
        }
    }
}

