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
    internal class 拉怪_队友飞斧 : Frost_WAR_ISlotResolver
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
            if (BattleData.以队友为目标的敌人列表.Count == 0)
            {
                return -3;
            }
            int baseCheck = CanUseGCD((uint)WARActionID.飞斧, 0, false, false);
            if (baseCheck != 0)
            {
                return baseCheck;
            }

            foreach (var enemy in BattleData.以队友为目标的敌人列表)
            {
                if (GameObjectExtension.Distance(Core.Me, enemy, DistanceMode.IgnoreAll) <= 20
                    && Core.Resolve<MemApiSpell>().CheckActionInRangeOrLoS((uint)WARActionID.飞斧, enemy))
                {
                    SpellID = (uint)WARActionID.飞斧;
                    target = enemy;
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog($"飞斧{enemy.Name}拉仇恨");
                    if (Setting.自动减伤使用提示)
                    {
                        Core.Resolve<MemApiChatMessage>().Toast2($"飞斧{enemy.Name}拉仇恨", 1, 2000);
                    }
                    if (!Frost_WAR_DutyData.Instance.无法拉仇恨的敌人列表.Contains(enemy))
                    {
                        Frost_WAR_DutyData.Instance.无法拉仇恨的敌人列表.Add(enemy);
                    }
                    return base.Check();
                }
            }

            return -404;
        }
    }
}

