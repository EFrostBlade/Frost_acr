using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Define;
using AEAssist.Extension;
using AEAssist.Helper;
using AEAssist.MemoryApi;
using Dalamud.Game.ClientState.Objects.Types;
using Frost.Frost_WAR.Frost_WAR_Data;

namespace Frost.Frost_WAR.Frost_WAR_SlotResolvers
{
    internal class 自动挑衅_拉怪 : Frost_WAR_ISlotResolver
    {
        public override int Check()
        {

            if (BattleData.ResolverState != 2
                && BattleData.ResolverState != 3
                && BattleData.ResolverState != 4)
            {
                return -1;
            }
            if (!Setting.自动挑衅)
            {
                return -4;
            }
            if ((Core.Resolve<MemApiMove>().IsMoving()))
            {
                return -2;
            }
            if (BattleData.以队友为目标的敌人列表.Count == 0)
            {
                return -3;
            }
            int baseCheck = CanUseDefenceOGCD((uint)WARActionID.挑衅);
            if (baseCheck != 0)
            {
                return baseCheck;
            }

            double cooldown = SpellHelper.GetSpell((uint)WARActionID.挑衅).Cooldown.TotalSeconds;
            if (cooldown > Setting.技能提前时间)
            {
                return -400;
            }
            foreach (var enemy in BattleData.以队友为目标的敌人列表)
            {
                if (GameObjectExtension.Distance(Core.Me, enemy, DistanceMode.IgnoreAll) <= 25
                    && Core.Resolve<MemApiSpell>().CheckActionInRangeOrLoS((uint)WARActionID.挑衅, enemy))
                {
                    SpellID = (uint)WARActionID.挑衅;
                    target = enemy;
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog($"挑衅{enemy.Name}w拉仇恨");
                    if (Setting.自动减伤使用提示)
                    {
                        Core.Resolve<MemApiChatMessage>().Toast2($"挑衅{enemy.Name}拉仇恨", 1, 2000);
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

