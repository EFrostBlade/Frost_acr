using AEAssist.CombatRoutine.Module.Target;
using AEAssist.CombatRoutine;
using AEAssist.Define;
using AEAssist.Extension;
using AEAssist.Helper;
using AEAssist.MemoryApi;
using AEAssist;
using Dalamud.Game.ClientState.Objects.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AEAssist.CombatRoutine.Module;
using Frost.Frost_PLD.Frost_PLD_Setting;
using static Frost.Frost_PLD.Frost_PLD_Data.SpellUtils;
using AEAssist.CombatRoutine.Module.AILoop;
using System.Numerics;
using Frost.Frost_PLD.Frost_PLD_Data;
using Dalamud.Plugin.Services;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using Frost.Common;
using Frost.Frost_PLD.Frost_PLD_Data;
using Frost.Frost_PLD.Frost_PLD_Data;
using Frost.Frost_PLD.Frost_PLD_Setting;
using Frost.Frost_PLD.Frost_PLD_Data;
using Frost.Frost_PLD.Frost_PLD_Setting;
using Frost.Frost_PLD.Frost_PLD_Data;
using Frost.Frost_PLD.Frost_PLD_Setting;

namespace Frost.Frost_PLD
{
    internal class Frost_PLD_UpdateACRData
    {
        public static void UpdateBattleData()
        {
            // Update Battle data here
            var battleData = Frost_PLD_BattleData.Instance;
            var QT = Frost_PLD_RotationEntry.JobViewWindow;
            battleData.目标距离 = GameObjectExtension.Distance(Core.Me, Core.Me.GetCurrTarget(), DistanceMode.IgnoreAll);
            battleData.目标是否在近战范围内 = battleData.目标距离 <= (float)SettingMgr.GetSetting<GeneralSettings>().AttackRange;
            battleData.自身血量记录.AddLast((Core.Me.CurrentHp, battleData.战斗开始时长));
            if (battleData.自身血量记录.Count > 12000)
            {
                battleData.自身血量记录.RemoveFirst();
            }
            CalculateDamagePerSecond(battleData.自身血量记录, battleData.战斗开始时长);
            UpdateDutyData();
            ulong myID = Core.Me.GameObjectId;
            IBattleChara? myTarget = Core.Me.GetCurrTarget() as IBattleChara;
            battleData.当前目标 = myTarget;
            battleData.战逃反应冷却时间 = SpellHelper.GetSpell((uint)PLDActionID.战逃反应).Cooldown.TotalSeconds;
            UpdateCombo();
            int targetMeCount = 0;
            var targetNullEnemy = new List<IBattleChara>();
            var targetMeEnemy = new List<IBattleChara>();
            var targePtartyEnemy = new List<IBattleChara>();
            var targetOtherEnemy = new List<IBattleChara>();
            var partyPlayer = new HashSet<ulong>();
            int enemyIn25 = 0;
            int enemyIn5 = 0;

            foreach (var member in PartyHelper.Party)
            {
                IBattleChara? payter = member as IBattleChara;
                if (payter != null && payter.GameObjectId != myID)
                {
                    partyPlayer.Add(payter.GameObjectId);
                }
            }

            foreach (var keyValuePair in TargetMgr.Instance.EnemysIn25)
            {
                IBattleChara? battleChara = keyValuePair.Value as IBattleChara;
                if (battleChara != null)
                {

                    if (GameObjectExtension.CanAttack(battleChara))
                    {
                        enemyIn25++;
                        if (battleChara.GetCurrTarget() == null)
                        {
                            targetNullEnemy.Add(battleChara);
                        }
                        else if (battleChara.TargetObjectId == myID)
                        {
                            targetMeCount++;
                            targetMeEnemy.Add(battleChara);
                            if (Frost_PLD_DutyData.Instance.无法拉仇恨的敌人列表.Contains(battleChara))
                            {
                                Frost_PLD_DutyData.Instance.无法拉仇恨的敌人列表.Remove(battleChara);
                            }
                        }
                        else if (partyPlayer.Contains(battleChara.TargetObjectId))
                        {
                            targePtartyEnemy.Add(battleChara);
                        }
                        else
                        {
                            targetOtherEnemy.Add(battleChara);
                        }
                        if (GameObjectExtension.Distance(Core.Me, battleChara, DistanceMode.IgnoreAll) <= 5)
                        {
                            enemyIn5++;
                        }
                    }
                }
            }

            battleData.以自身为目标的敌人数量 = targetMeCount;
            battleData.无目标的敌人列表 = targetNullEnemy;
            battleData.以自身为目标的敌人列表 = targetMeEnemy;
            battleData.以队友为目标的敌人列表 = targePtartyEnemy;
            battleData.以其他为目标的敌人列表 = targetOtherEnemy;
            if (QT.GetQt("双目标aoe"))
            {
                if (enemyIn5 >= 2 || TargetHelper.GetNearbyEnemyCount(5) >= 2)
                {
                    battleData.是否可打aoe = true;
                }
                else
                {
                    battleData.是否可打aoe = false;
                }
            }
            else
            {
                {
                    if (enemyIn5 >= 3 || TargetHelper.GetNearbyEnemyCount(5) >= 3)
                    {
                        battleData.是否可打aoe = true;
                    }
                    else
                    {
                        battleData.是否可打aoe = false;
                    }
                }
            }
            UpdateBattleState(myTarget, enemyIn25, enemyIn5);
            UpdateStatusState(myTarget);
        }
        public static void CalculateDamagePerSecond(LinkedList<(uint, long)> hpRecords, long currentTime)
        {
            var damagePerSecond = new Dictionary<int, int>
            {
                { 5, 0 },
                { 4, 0 },
                { 3, 0 },
                { 2, 0 },
                { 1, 0 }
            };

            List<(uint, long)> hpRecordsList = hpRecords.ToList();
            for (int i = hpRecordsList.Count - 1; i > 0; i--)
            {
                (uint hp, long time) = hpRecordsList[i];
                (uint prevHp, long prevTime) = hpRecordsList[i - 1];

                long timeDiff = currentTime - time;
                int damage = (int)prevHp - (int)hp;

                if (timeDiff <= 5 * 1000)
                {
                    var second = (int)Math.Ceiling((double)timeDiff / 1000);
                    if (damagePerSecond.ContainsKey(second))
                    {
                        damagePerSecond[second] += damage;
                    }
                }
            }

            Frost_PLD_BattleData.Instance.自身每秒承伤 = damagePerSecond;
        }
        private static void UpdateBattleState(IBattleChara? myTarget, int enemyIn25, int enemyIn5)
        {
            Frost_PLD_BattleData.Instance.上次战斗状态 = Frost_PLD_BattleData.Instance.当前战斗状态;
            var dutyData = Frost_PLD_DutyData.Instance;

            if (dutyData.是否高难)
            {
                UpdateBattleStateForHighDifficulty(myTarget);
            }
            else if (dutyData.是否在副本中
                && (dutyData.副本人数 == 4 || dutyData.副本人数 == 8 || dutyData.副本人数 == 24))
            {
                UpdateBattleStateForDuty(myTarget, enemyIn25, enemyIn5);
            }
            else
            {
                UpdateBattleStateForNonDuty(myTarget);
            }
        }

        private static void UpdateBattleStateForNonDuty(IBattleChara? myTarget)
        {
            var battleData = Frost_PLD_BattleData.Instance;
            if (Core.Me.InCombat() || battleData.isChecking)
            {
                if (myTarget != null)
                {
                    if (myTarget.IsBoss())
                    {
                        battleData.当前战斗状态 = TTKHelper.IsTargetTTK(myTarget)
                            ? 战斗状态.野外BOSS战斗即将结束
                            : 战斗状态.野外BOSS战斗中;
                    }
                    else
                    {
                        int ttkCount = battleData.以自身为目标的敌人列表.Count(battleChara => TTKHelper.IsTargetTTK(battleChara) || battleChara.CurrentHpPercent() < 0.2f);
                        battleData.当前战斗状态 = ttkCount >= battleData.以自身为目标的敌人数量
                            ? 战斗状态.野外小怪战斗即将结束
                            : 战斗状态.野外小怪战斗中;
                    }
                }
            }
            else
            {
                battleData.当前战斗状态 = 战斗状态.无战斗;
            }
        }

        private static void UpdateBattleStateForHighDifficulty(IBattleChara? myTarget)
        {
            var battleData = Frost_PLD_BattleData.Instance;
            if (Core.Me.InCombat() || battleData.isChecking)
            {
                if (myTarget != null)
                {
                    if (Core.Resolve<MemApiDuty>().InBossBattle || myTarget.IsBoss())
                    {
                        battleData.当前战斗状态 = TTKHelper.IsTargetTTK(myTarget)
                            ? 战斗状态.高难本BOSS战斗即将结束
                            : 战斗状态.高难本BOSS战斗中;
                    }
                }
            }
            else
            {
                battleData.当前战斗状态 = 战斗状态.无战斗;
            }
        }

        private static void UpdateBattleStateForDuty(IBattleChara? myTarget, int enemyIn25, int enemyIn5)
        {
            var battleData = Frost_PLD_BattleData.Instance;
            if (Core.Me.InCombat() || battleData.isChecking)
            {
                if (myTarget != null)
                {
                    if (Core.Resolve<MemApiDuty>().InBossBattle || myTarget.IsBoss())
                    {
                        UpdateBattleStateForBoss(myTarget);
                    }
                    else
                    {
                        UpdateBattleStateForNonBoss(enemyIn25, enemyIn5);
                    }
                }
            }
            else
            {
                battleData.当前战斗状态 = 战斗状态.无战斗;
            }
        }

        private static void UpdateBattleStateForBoss(IBattleChara myTarget)
        {
            var battleData = Frost_PLD_BattleData.Instance;
            var dutyData = Frost_PLD_DutyData.Instance;
            switch (dutyData.副本人数)
            {
                case 4:
                    battleData.当前战斗状态 = TTKHelper.IsTargetTTK(myTarget)
                        ? 战斗状态.四人本BOSS战斗即将结束
                        : 战斗状态.四人本BOSS战斗中;
                    break;
                case 8:
                    battleData.当前战斗状态 = TTKHelper.IsTargetTTK(myTarget)
                        ? 战斗状态.八人本BOSS战斗即将结束
                        : 战斗状态.八人本BOSS战斗中;
                    break;
                case 24:
                    battleData.当前战斗状态 = TTKHelper.IsTargetTTK(myTarget)
                        ? 战斗状态.团本BOSS战斗即将结束
                        : 战斗状态.团本BOSS战斗中;
                    break;
            }
        }

        private static void UpdateBattleStateForNonBoss(int enemyIn25, int enemyIn5)
        {
            var battleData = Frost_PLD_BattleData.Instance;
            var dutyData = Frost_PLD_DutyData.Instance;
            switch (dutyData.副本人数)
            {
                case 4:
                    if ((enemyIn25 > enemyIn5 + Frost_PLD_DutyData.Instance.无法拉仇恨的敌人列表.Count() && Core.Resolve<MemApiMove>().IsMoving())
                        || battleData.以队友为目标的敌人列表.Count() > 0)
                    {
                        battleData.当前战斗状态 = 战斗状态.四人本拉怪途中;
                    }
                    else
                    {
                        int ttkCount = battleData.以自身为目标的敌人列表.Count(battleChara => TTKHelper.IsTargetTTK(battleChara) || battleChara.CurrentHpPercent() < 0.2f);
                        battleData.当前战斗状态 = ttkCount >= battleData.以自身为目标的敌人数量
                            ? 战斗状态.四人本小怪战斗即将结束
                            : 战斗状态.四人本小怪战斗中;
                    }
                    break;
                case 8:
                    int ttkCount8 = battleData.以自身为目标的敌人列表.Count(battleChara => TTKHelper.IsTargetTTK(battleChara) || battleChara.CurrentHpPercent() < 0.2f);
                    battleData.当前战斗状态 = ttkCount8 >= battleData.以自身为目标的敌人数量
                        ? 战斗状态.八人本小怪战斗即将结束
                        : 战斗状态.八人本小怪战斗中;
                    break;
                case 24:
                    int ttkCount24 = battleData.以自身为目标的敌人列表.Count(battleChara => TTKHelper.IsTargetTTK(battleChara) || battleChara.CurrentHpPercent() < 0.2f);
                    battleData.当前战斗状态 = ttkCount24 >= battleData.以自身为目标的敌人数量
                        ? 战斗状态.团本小怪战斗即将结束
                        : 战斗状态.团本小怪战斗中;
                    break;
            }
        }

        public static void UpdateDutyData(bool log = false)
        {
            var dutyData = Frost_PLD_DutyData.Instance;
            DutyInfo dutyInfo = new DutyInfo();
            dutyData.是否在副本中 = dutyInfo.InDuty;
            dutyData.副本人数 = dutyInfo.MemberNum;
        }

        public static void UpdateStatusState(IBattleChara? myTarget)
        {
            var battleData = Frost_PLD_BattleData.Instance;
            bool 是否开盾 = false;
            bool 是否战技封印 = false;
            bool 是否能力封印 = false;
            double 自身承伤 = 100;
            double 队伍承伤 = 100;
            int 自身护盾 = 0;
            int 队伍护盾 = 0;
            string 持有单减 = "";
            string 持有群减 = "";
            string 持有单盾 = "";
            string 持有群盾 = "";
            string 持有目标减 = "";
            string buff列表 = "";
            HashSet<string> 已有buff = new HashSet<string>();
            HashSet<string> 目标buff = new HashSet<string>();
            battleData.战逃反应剩余时间 = 0;
            battleData.安魂祈祷剩余时间 = 0;
            battleData.安魂祈祷层数 = 0;
            battleData.悔罪预备剩余时间 = 0;
            battleData.爆发药剩余时间 = 0;
            battleData.沥血剑预备剩余时间 = 0;
            battleData.赎罪剑预备剩余时间 = 0;
            battleData.祈告剑预备剩余时间 = 0;
            battleData.葬送剑预备剩余时间 = 0;
            battleData.神圣领域剩余时间 = 0;
            battleData.神圣魔法效果提高剩余时间 = 0;

            if (SpellHelper.GetSpell((uint)PLDActionID.预警).RecentlyUsed())
            {
                已有buff.Add("预警");
                自身承伤 *= (1 - 30 / 100.0);
                持有单减 += "预警" + ",";
            }
            if (SpellHelper.GetSpell((uint)PLDActionID.铁壁).RecentlyUsed())
            {
                已有buff.Add("铁壁");
                自身承伤 *= (1 - 20 / 100.0);
                持有单减 += "铁壁" + ",";
            }
            if (SpellHelper.GetSpell((uint)PLDActionID.雪仇).RecentlyUsed(3000))
            {
                目标buff.Add("雪仇");
                队伍承伤 *= (1 - 10 / 100.0);
                持有目标减 += "雪仇" + ",";
            }
            if (SpellHelper.GetSpell((uint)PLDActionID.亲疏自行).RecentlyUsed(3000))
            {
                目标buff.Add("减速");
                队伍承伤 *= (1 - 15 / 100.0);
                持有目标减 += "减速" + ",";
            }
            if (SpellHelper.GetSpell((uint)PLDActionID.极致防御).RecentlyUsed())
            {
                已有buff.Add("极致防御");
                自身承伤 *= (1 - 40 / 100.0);
                持有单减 += "极致防御" + ",";
            }
            else if (SpellHelper.GetSpell((uint)PLDActionID.壁垒).RecentlyUsed())
            {
                已有buff.Add("壁垒");
                自身承伤 *= (1 - 15 / 100.0);
                持有单减 += "壁垒" + ",";
            }
            if (SpellHelper.GetSpell((uint)PLDActionID.盾阵).RecentlyUsed())
            {
                已有buff.Add("盾阵");
                自身承伤 *= (1 - 15 / 100.0);
                持有单减 += "盾阵" + ",";
            }
            else if (SpellHelper.GetSpell((uint)PLDActionID.圣盾阵).RecentlyUsed())
            {
                已有buff.Add("圣盾阵");
                自身承伤 *= (1 - 15 / 100.0);
                持有单减 += "圣盾阵" + ",";
                已有buff.Add("骑士的坚守");
                自身承伤 *= (1 - 15 / 100.0);
                持有单减 += "骑士的坚守" + ",";
            }
            if (SpellHelper.GetSpell((uint)PLDActionID.圣光幕帘).RecentlyUsed())
            {
                已有buff.Add("圣光幕帘");
                队伍承伤 *= (1 - 10 / 100.0);
                持有群减 += "圣光幕帘" + ",";
            }
            if (SpellHelper.GetSpell((uint)PLDActionID.武装戍卫).RecentlyUsed())
            {
                已有buff.Add("武装戍卫");
                队伍承伤 *= (1 - 15 / 100.0);
                持有群减 += "武装戍卫" + ",";
            }
            for (int i = 0; i < Core.Me.StatusList.Length; i++)
            {
                Dalamud.Game.ClientState.Statuses.Status status = Core.Me.StatusList[i];
                if (status == null || status.StatusId == 0)
                {
                    continue;
                }

                if (!status.GameData.Value.Name.ToString().Contains("部队特效"))
                {
                    buff列表 += $"{status.StatusId}:{status.GameData.Value.Name} ";
                    if (status.StatusId == (uint)PLDBuffID.强化药)
                    {
                        battleData.爆发药剩余时间 = Math.Abs(status.RemainingTime);
                    }
                    if (status.StatusId == (uint)PLDBuffID.战逃反应)
                    {
                        battleData.战逃反应剩余时间 = Math.Abs(status.RemainingTime);
                    }
                    if (status.StatusId == (uint)PLDBuffID.安魂祈祷)
                    {
                        battleData.安魂祈祷剩余时间 = Math.Abs(status.RemainingTime);
                        battleData.安魂祈祷层数 = status.StackCount;
                    }
                    if (status.StatusId == (uint)PLDBuffID.悔罪预备)
                    {
                        battleData.悔罪预备剩余时间 = Math.Abs(status.RemainingTime);
                    }
                    if (status.StatusId == (uint)PLDBuffID.沥血剑预备)
                    {
                        battleData.沥血剑预备剩余时间 = Math.Abs(status.RemainingTime);
                    }
                    if (status.StatusId == (uint)PLDBuffID.赎罪剑预备)
                    {
                        battleData.赎罪剑预备剩余时间 = Math.Abs(status.RemainingTime);
                    }
                    if (status.StatusId == (uint)PLDBuffID.祈告剑预备)
                    {
                        battleData.祈告剑预备剩余时间 = Math.Abs(status.RemainingTime);
                    }
                    if (status.StatusId == (uint)PLDBuffID.葬送剑预备)
                    {
                        battleData.葬送剑预备剩余时间 = Math.Abs(status.RemainingTime);
                    }
                    if (status.StatusId == (uint)PLDBuffID.神圣魔法效果提高)
                    {
                        battleData.神圣魔法效果提高剩余时间 = Math.Abs(status.RemainingTime);
                    }
                    if (status.StatusId == (uint)PLDBuffID.神圣领域)
                    {
                        battleData.神圣领域剩余时间 = Math.Abs(status.RemainingTime);
                    }
                    else if (status.StatusId == (uint)PLDBuffID.钢铁信念)
                    {
                        是否开盾 = true;
                        battleData.是否开盾 = true;
                    }
                    else if (status.StatusId == (uint)PLDBuffID.战技封印1)
                    {
                        是否战技封印 = true;
                        battleData.是否战技封印 = true;
                    }
                    else if (status.StatusId == (uint)PLDBuffID.战技封印2)
                    {
                        是否战技封印 = true;
                        battleData.是否战技封印 = true;
                    }
                    else if (status.StatusId == (uint)PLDBuffID.能力封印1)
                    {
                        是否能力封印 = true;
                        battleData.是否能力封印 = true;
                    }
                    else if (status.StatusId == (uint)PLDBuffID.能力封印2)
                    {
                        是否能力封印 = true;
                        battleData.是否能力封印 = true;
                    }
                    else if (status.StatusId == (uint)PLDBuffID.能力封印3)
                    {
                        是否能力封印 = true;
                        battleData.是否能力封印 = true;
                    }
                    else
                    {
                        DamageReductionInfo buff = DamageReduction.DamageReductionBuffs.FirstOrDefault(b => b.Name == status.GameData.Value.Name);
                        if (buff != null && !已有buff.Contains(buff.Name))
                        {
                            已有buff.Add(buff.Name);
                            if (buff.Mode == 0)
                            {
                                自身承伤 *= (1 - buff.Reduction / 100.0);
                                持有单减 += buff.Name + ",";
                            }
                            else if (buff.Mode == 2)
                            {
                                队伍承伤 *= (1 - buff.Reduction / 100.0);
                                持有群减 += buff.Name + ",";
                            }
                            else if (buff.Mode == 3)
                            {
                                自身护盾 += buff.Reduction;
                                持有单盾 += buff.Name + ",";
                            }
                            else if (buff.Mode == 4)
                            {
                                队伍护盾 += buff.Reduction;
                                持有群盾 += buff.Name + ",";
                            }
                        }
                    }
                }
            }
            if (!是否开盾)
            {
                battleData.是否开盾 = false;
            }
            if (!是否战技封印)
            {
                battleData.是否战技封印 = false;
            }
            if (!是否能力封印)
            {
                battleData.是否能力封印 = false;
            }
            if (myTarget != null)
            {

                for (int i = 0; i < myTarget.StatusList.Length; i++)
                {
                    Dalamud.Game.ClientState.Statuses.Status status = myTarget.StatusList[i];
                    if (status == null || status.StatusId == 0)
                    {
                        continue;
                    }
                    if (status != null)
                    {
                        DamageReductionInfo buff = DamageReduction.DamageReductionBuffs.FirstOrDefault(b => b.Name == status.GameData.Value.Name);
                        if (buff != null && !目标buff.Contains(buff.Name))
                        {
                            目标buff.Add(buff.Name);
                            if (buff.Mode == 1)
                            {
                                队伍承伤 *= (1 - buff.Reduction / 100.0);
                                持有目标减 += buff.Name + ",";
                            }
                        }
                    }
                }
            }
            int 自身减伤 = 100 - (int)(自身承伤 * 队伍承伤 / 100);
            int 队伍减伤 = 100 - (int)队伍承伤;
            自身护盾 += 队伍护盾;
            battleData.自身减伤比例 = 自身减伤;
            battleData.队伍减伤比例 = 队伍减伤;
            battleData.自身护盾 = 自身护盾;
            battleData.队伍护盾 = 队伍护盾;
            battleData.持有单减 = 持有单减;
            battleData.持有群减 = 持有群减;
            battleData.持有单盾 = 持有单盾;
            battleData.持有群盾 = 持有群盾;
            battleData.持有目标减 = 持有目标减;
            battleData.buff列表 = buff列表;
        }
        public static void ModifySlotResolvers(List<SlotResolverData> slotResolvers)
        {
            var battleData = Frost_PLD_BattleData.Instance;
            var QT = Frost_PLD_RotationEntry.JobViewWindow;
            if (QT.GetQt("停手"))
            {
                //Frost_PLD_RotationEntry.Frost_PLD_ArcUI.AddLog("切换至停手");
                battleData.ResolverState = -1;
            }
            else
            {
                //BOSS快打完泄资源
                if (!battleData.已开泄资源
                    && (battleData.当前战斗状态 == 战斗状态.四人本BOSS战斗即将结束
                    || battleData.当前战斗状态 == 战斗状态.八人本BOSS战斗即将结束
                    || battleData.当前战斗状态 == 战斗状态.团本BOSS战斗即将结束
                    || battleData.当前战斗状态 == 战斗状态.野外BOSS战斗中
                    || battleData.当前战斗状态 == 战斗状态.野外BOSS战斗即将结束
                    || battleData.当前战斗状态 == 战斗状态.高难本BOSS战斗即将结束))
                {
                    if (battleData.当前目标 != null
                        && (battleData.当前目标.DataId != 901) && (battleData.当前目标.DataId != 16845) && (battleData.当前目标.Name.ToString() != "木人"))
                    {
                        QT.SetQt("泄资源", true);
                        battleData.已开泄资源 = true;
                        if (battleData.当前战斗状态 == 战斗状态.野外BOSS战斗中
                        || battleData.当前战斗状态 == 战斗状态.野外BOSS战斗即将结束)
                        {
                            QT.SetQt("没红斩也泄", true);
                        }
                    }
                }
                if (battleData.当前战斗状态 == 战斗状态.无战斗)
                {
                    battleData.ResolverState = 0;
                }
                // 泄资源逻辑
                if (QT.GetQt("泄资源"))
                {
                    //tempSlotResolver.Add(new SlotResolverData(new Frost_PLD_泄资源_Defence_SlotResolvers(), SlotMode.OffGcd));
                    //tempSlotResolver.Add(new SlotResolverData(new Frost_PLD_泄资源_OGCD_SlotResolvers(), SlotMode.OffGcd));
                    //tempSlotResolver.Add(new SlotResolverData(new Frost_PLD_泄资源_GCD_SlotResolvers(), SlotMode.Gcd));
                    //Frost_PLD_RotationEntry.Frost_PLD_ArcUI.AddLog("切换至泄资源状态");

                    battleData.ResolverState = 1;
                }
                //拉怪途中逻辑
                else if (battleData.当前战斗状态 == 战斗状态.四人本拉怪途中)
                {
                    //tempSlotResolver.Add(new SlotResolverData(new Frost_PLD_拉怪_Defence_SlotResolvers(), SlotMode.OffGcd));
                    //tempSlotResolver.Add(new SlotResolverData(new Frost_PLD_拉怪_OGCD_SlotResolvers(), SlotMode.OffGcd));
                    //tempSlotResolver.Add(new SlotResolverData(new Frost_PLD_拉怪_GCD_SlotResolvers(), SlotMode.Gcd));
                    battleData.ResolverState = 2;
                }
                // 小怪战斗即将结束逻辑
                else if (battleData.当前战斗状态 == 战斗状态.四人本小怪战斗即将结束
                    || battleData.当前战斗状态 == 战斗状态.八人本小怪战斗即将结束
                    || battleData.当前战斗状态 == 战斗状态.团本小怪战斗即将结束)
                {
                    //tempSlotResolver.Add(new SlotResolverData(new Frost_PLD_小怪战斗即将结束_Defence_SlotResolvers(), SlotMode.OffGcd));
                    //tempSlotResolver.Add(new SlotResolverData(new Frost_PLD_小怪战斗即将结束_OGCD_SlotResolvers(), SlotMode.OffGcd));
                    //tempSlotResolver.Add(new SlotResolverData(new Frost_PLD_小怪战斗即将结束_GCD_SlotResolvers(), SlotMode.Gcd));
                    //Frost_PLD_RotationEntry.Frost_PLD_ArcUI.AddLog("切换至小怪战斗即将结束状态");
                    battleData.ResolverState = 4;
                }
                //小怪战斗中逻辑
                else if (battleData.当前战斗状态 == 战斗状态.四人本小怪战斗中
                    || battleData.当前战斗状态 == 战斗状态.八人本小怪战斗中
                    || battleData.当前战斗状态 == 战斗状态.野外小怪战斗中
                    || battleData.当前战斗状态 == 战斗状态.野外小怪战斗即将结束
                    || battleData.当前战斗状态 == 战斗状态.团本小怪战斗中)
                {
                    //tempSlotResolver.Add(new SlotResolverData(new Frost_PLD_小怪战斗中_Defence_SlotResolvers(), SlotMode.OffGcd));
                    //tempSlotResolver.Add(new SlotResolverData(new Frost_PLD_小怪战斗中_OGCD_SlotResolvers(), SlotMode.OffGcd));
                    //tempSlotResolver.Add(new SlotResolverData(new Frost_PLD_小怪战斗中_GCD_SlotResolvers(), SlotMode.Gcd));
                    battleData.ResolverState = 3;
                }
                //BOSS战斗中逻辑
                else if (battleData.当前战斗状态 == 战斗状态.四人本BOSS战斗中
                    || battleData.当前战斗状态 == 战斗状态.八人本BOSS战斗中
                    || battleData.当前战斗状态 == 战斗状态.团本BOSS战斗中
                    || battleData.当前战斗状态 == 战斗状态.野外BOSS战斗中
                    || battleData.当前战斗状态 == 战斗状态.高难本BOSS战斗中)
                {
                    //tempSlotResolver.Add(new SlotResolverData(new Frost_PLD_BOSS战斗中_Defence_SlotResolvers(), SlotMode.OffGcd));
                    //tempSlotResolver.Add(new SlotResolverData(new Frost_PLD_BOSS战斗中_OGCD_SlotResolvers(), SlotMode.OffGcd));
                    //tempSlotResolver.Add(new SlotResolverData(new Frost_PLD_BOSS战斗中_GCD_SlotResolvers(), SlotMode.Gcd));
                    //Frost_PLD_RotationEntry.Frost_PLD_ArcUI.AddLog("切换至BOSS战斗中状态");
                    battleData.ResolverState = 5;
                }
            }
        }
        public static async Task OnNoCombat()
        {
            await SpellCast();
            if (Share.Pull)
            {
                if (Core.Me.TargetObject != null)
                {
                    IBattleChara? target = Core.Me.TargetObject as IBattleChara;
                    if (target != null)
                    {
                        if (GameObjectExtension.CanAttack(target))
                        {
                            if (GameObjectExtension.Distance(Core.Me, Core.Me.GetCurrTarget(), DistanceMode.IgnoreAll) <= (float)SettingMgr.GetSetting<GeneralSettings>().AttackRange)
                            {
                                if (CanUseOGCD_NoCombat((uint)PLDActionID.先锋剑))
                                {
                                    await SpellHelper.GetSpell((uint)PLDActionID.先锋剑, SpellTargetType.Target).Cast();
                                }
                            }
                            else if (GameObjectExtension.Distance(Core.Me, Core.Me.GetCurrTarget(), DistanceMode.IgnoreAll) <= 20)
                            {
                                if (CanUseOGCD_NoCombat((uint)PLDActionID.投盾))
                                {
                                    if (Core.Resolve<MemApiSpell>().CheckActionInRangeOrLoS((uint)PLDActionID.投盾, target))
                                    {
                                        await SpellHelper.GetSpell((uint)PLDActionID.投盾, SpellTargetType.Target).Cast();
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }


        public static async Task SpellCast()
        {
            var battleData = Frost_PLD_BattleData.Instance;
            var QT = Frost_PLD_RotationEntry.JobViewWindow;
            var SC = Frost_PLD_RotationEntry.scWindow;
            if (Frost_PLD_Settings.Instance.启用qt控制盾姿)
            {
                bool isShieldActive = GameObjectExtension.HasAura(Core.Me, (uint)PLDBuffID.钢铁信念);
                bool shouldActivateShield = Frost_PLD_RotationEntry.JobViewWindow.GetQt("盾姿");
                bool isShieldColldown = SpellHelper.GetSpell((uint)PLDActionID.钢铁信念).Cooldown.TotalSeconds == 0 && SpellHelper.GetSpell((uint)PLDActionID.解除钢铁信念).Cooldown.TotalSeconds == 0;

                if (isShieldColldown && shouldActivateShield && !isShieldActive && !SpellHelper.GetSpell((uint)PLDActionID.钢铁信念).RecentlyUsed() && !SpellHelper.GetSpell((uint)PLDActionID.解除钢铁信念).RecentlyUsed())
                {
                    Frost_PLD_RotationEntry.Frost_PLD_ArcUI.AddLog("战斗外开盾");
                    await SpellHelper.GetSpell((uint)PLDActionID.钢铁信念).Cast();
                }
                else if (isShieldColldown && !shouldActivateShield && isShieldActive && !SpellHelper.GetSpell((uint)PLDActionID.解除钢铁信念).RecentlyUsed() && !SpellHelper.GetSpell((uint)PLDActionID.钢铁信念).RecentlyUsed())
                {
                    Frost_PLD_RotationEntry.Frost_PLD_ArcUI.AddLog("战斗外关盾");
                    await SpellHelper.GetSpell((uint)PLDActionID.解除钢铁信念).Cast();
                }
            }
            if (Core.Resolve<MemApiCondition>().IsBoundByDuty()
                && Core.Resolve<MemApiMove>().IsMoving()
                && Frost_PLD_Settings.Instance.自动疾跑)
            {
                if (Frost_PLD_DutyData.Instance.副本人数 == 4 || Frost_PLD_DutyData.Instance.副本人数 == 24)
                {
                    if (DateTime.Now - Frost_PLD_DutyData.Instance.上次疾跑时间 > TimeSpan.FromSeconds(60))
                    {
                        if (CanUseOGCD_NoCombat((uint)PLDActionID.冲刺))
                        {
                            await SpellHelper.GetSpell((uint)PLDActionID.冲刺).Cast();
                            Frost_PLD_DutyData.Instance.上次疾跑时间 = DateTime.Now;
                        }
                    }
                }
            }
            if (Core.Resolve<MemApiCondition>().IsBoundByDuty()
                && Core.Resolve<MemApiMove>().IsMoving()
                && Frost_PLD_Settings.Instance.自动拉怪突进)
            {
                if (Frost_PLD_DutyData.Instance.副本人数 == 4 || Frost_PLD_DutyData.Instance.副本人数 == 24)
                {
                    if (TargetMgr.Instance.EnemysIn20 != null)
                    {
                        // 创建一个EnemysIn20的副本，避免在遍历时集合被修改导致异常
                        var enemyList = new Dictionary<uint, IBattleChara>(TargetMgr.Instance.EnemysIn20);
                        foreach (var keyValuePair in enemyList)
                        {
                            if (CanUseOGCD_NoCombat((uint)PLDActionID.调停))
                            {
                                IBattleChara enemy = keyValuePair.Value;
                                if (GameObjectExtension.Distance(Core.Me, enemy, DistanceMode.IgnoreAll) > 12
                                    && Core.Resolve<MemApiSpell>().CheckActionInRangeOrLoS((uint)PLDActionID.调停, enemy))
                                {
                                    // 计算目标是否在当前面向范围内
                                    var directionToTarget = enemy.Position - Core.Me.Position;
                                    var forPLDdDirection = new Vector3((float)Math.Sin(Core.Me.Rotation), 0, (float)Math.Cos(Core.Me.Rotation));
                                    var angle = Math.Acos(Vector3.Dot(Vector3.Normalize(forPLDdDirection), Vector3.Normalize(directionToTarget))) * (180.0 / Math.PI);

                                    if (angle <= 40)
                                    {
                                        Slot slot = new Slot();
                                        slot.Add(new Spell((uint)PLDActionID.调停, enemy));
                                        await slot.Run(AI.Instance.BattleData, isNextSlot: false);
                                        Frost_PLD_RotationEntry.Frost_PLD_ArcUI.AddLog($"自动突进{enemy.Name}拉怪");
                                        Core.Resolve<MemApiChatMessage>().Toast2($"自动突进{enemy.Name}拉怪", 1, 2000);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (SC.GetSC("神圣领域"))
            {
                if (CanUseOGCD_NoCombat((uint)PLDActionID.神圣领域))
                {
                    await SpellHelper.GetSpell((uint)PLDActionID.神圣领域).Cast();
                }
            }
            if (SC.GetSC("极致防御"))
            {
                if (Core.Me.Level >= 92)
                {
                    if (CanUseOGCD_NoCombat((uint)PLDActionID.极致防御))
                    {
                        await SpellHelper.GetSpell((uint)PLDActionID.极致防御).Cast();
                    }
                }
                else
                {
                    if (CanUseOGCD_NoCombat((uint)PLDActionID.预警))
                    {
                        await SpellHelper.GetSpell((uint)PLDActionID.预警).Cast();
                    }
                }
            }
            if (SC.GetSC("铁壁"))
            {
                if (CanUseOGCD_NoCombat((uint)PLDActionID.铁壁))
                {
                    await SpellHelper.GetSpell((uint)PLDActionID.铁壁).Cast();
                }
            }
            if (SC.GetSC("壁垒"))
            {
                if (CanUseOGCD_NoCombat((uint)PLDActionID.壁垒))
                {
                    await SpellHelper.GetSpell((uint)PLDActionID.壁垒).Cast();
                }
            }
            if (SC.GetSC("圣盾阵"))
            {
                if (Core.Me.Level >= 82)
                {
                    if (CanUseOGCD_NoCombat((uint)PLDActionID.圣盾阵))
                    {
                        await SpellHelper.GetSpell((uint)PLDActionID.圣盾阵).Cast();
                    }
                }
                else
                {
                    if (CanUseOGCD_NoCombat((uint)PLDActionID.盾阵))
                    {
                        await SpellHelper.GetSpell((uint)PLDActionID.盾阵).Cast();
                    }
                }
            }
            if (SC.GetSC("雪仇"))
            {
                if (CanUseOGCD_NoCombat((uint)PLDActionID.雪仇))
                {
                    await SpellHelper.GetSpell((uint)PLDActionID.雪仇).Cast();
                }
            }
            if (SC.GetSC("圣光幕帘"))
            {
                if (CanUseOGCD_NoCombat((uint)PLDActionID.圣光幕帘))
                {
                    await SpellHelper.GetSpell((uint)PLDActionID.圣光幕帘).Cast();
                }
            }
            if (SC.GetSC("亲疏自行"))
            {
                if (CanUseOGCD_NoCombat((uint)PLDActionID.亲疏自行))
                {
                    await SpellHelper.GetSpell((uint)PLDActionID.亲疏自行).Cast();
                }
            }
            if (SC.GetSC("武装戍卫"))
            {
                if (CanUseOGCD_NoCombat((uint)PLDActionID.武装戍卫))
                {

                    // 通过 Core.Me 获取自身位置，其中 Position 为 Vector3 类型
                    var self = Core.Me;
                    Vector3 selfPos = self.Position;
                    var allies = PartyHelper.CastableAlliesWithin10;
                    int bestCount = 0;
                    float bestAngle = 0;
                    List<IBattleChara> bestEffectAlly = new List<IBattleChara>();

                    // 遍历候选角度 0 ～ 359 度
                    for (int candidateDeg = 0; candidateDeg < 360; candidateDeg++)
                    {
                        float rad = candidateDeg * MathF.PI / 180;
                        // 构造候选方向向量（忽略 Y 轴高度，只考虑 XZ 平面）
                        Vector3 candidateDir = new Vector3(MathF.Cos(rad), 0, MathF.Sin(rad));
                        int count = 0;
                        List<IBattleChara> effectAlly = new List<IBattleChara>();
                        foreach (var ally in allies)
                        {
                            // 计算从自身到队友的向量
                            Vector3 diff = ally.Position - selfPos;
                            if (diff.Length() > 8f)
                                continue;
                            diff = Vector3.Normalize(diff);
                            // 计算候选方向和队友方向的夹角（单位：度）
                            float dot = Vector3.Dot(candidateDir, diff);
                            float angleDeg = MathF.Acos(dot) * 180 / MathF.PI;
                            // 检测队友是否落在候选扇形内（左右各 60°）
                            if (angleDeg <= 60)
                            {
                                count++;
                                effectAlly.Add(ally);
                            }

                        }
                        if (count > bestCount)
                        {
                            bestCount = count;
                            bestAngle = candidateDeg;
                            bestEffectAlly = effectAlly;
                        }
                    }
                    string log = $"最佳释放角度{bestAngle} 队友数量{bestCount}";
                    string message = $"将覆盖{bestCount}名队友:";
                    if (bestEffectAlly.Count > 0)
                    {
                        foreach (var ally in bestEffectAlly)
                        {
                            log += $" {ally.Name}";
                            message += $"{ally.Name} ";
                        }
                    }
                    Frost_PLD_RotationEntry.Frost_PLD_ArcUI.AddLog(log);
                    LogHelper.Print("武装戍卫", message);


                    // 将最佳面向转换为 SetRot 所需要的弧度值（0代表正南，π/2代表正东，-π/2代表正西）
                    float candidateRad = bestAngle * MathF.PI / 180f;
                    float setRot = candidateRad - 3f * MathF.PI / 2f;
                    // 归一化到 [-π, π]
                    while (setRot > MathF.PI)
                        setRot -= 2f * MathF.PI;
                    while (setRot < -MathF.PI)
                        setRot += 2f * MathF.PI;
                    Core.Resolve<MemApiMove>().SetRot(setRot);
                    await SpellHelper.GetSpell((uint)PLDActionID.武装戍卫).Cast();
                }
            }
            if (SC.GetSC("调停"))
            {
                if (CanUseOGCD_NoCombat((uint)PLDActionID.调停))
                {
                    IBattleChara? _target = SC.GetSCTarget("调停");
                    if (_target != null)
                    {
                        if (_target.DistanceToPlayer() <= 20)
                        {
                            if (Core.Resolve<MemApiSpell>().CheckActionInRangeOrLoS((uint)PLDActionID.调停, _target))
                            {
                                await SpellHelper.GetSpell((uint)PLDActionID.调停, _target).Cast();
                            }
                        }
                    }
                }
            }
            if (SC.GetSC("下踢"))
            {
                if (CanUseOGCD_NoCombat((uint)PLDActionID.下踢))
                {
                    IBattleChara? _target = SC.GetSCTarget("下踢");
                    if (_target != null)
                    {
                        if (_target.DistanceToPlayer() <= (float)SettingMgr.GetSetting<GeneralSettings>().AttackRange)
                        {
                            if (Core.Resolve<MemApiSpell>().CheckActionInRangeOrLoS((uint)PLDActionID.下踢, _target))
                            {
                                await SpellHelper.GetSpell((uint)PLDActionID.下踢, _target).Cast();
                            }
                        }
                    }
                }
            }
            if (SC.GetSC("插言"))
            {
                if (CanUseOGCD_NoCombat((uint)PLDActionID.插言))
                {
                    IBattleChara? _target = SC.GetSCTarget("插言");
                    if (_target != null)
                    {
                        if (_target.DistanceToPlayer() <= (float)SettingMgr.GetSetting<GeneralSettings>().AttackRange)
                        {
                            if (Core.Resolve<MemApiSpell>().CheckActionInRangeOrLoS((uint)PLDActionID.插言, _target))
                            {
                                await SpellHelper.GetSpell((uint)PLDActionID.插言, _target).Cast();
                            }
                        }
                    }
                }
            }
            if (SC.GetSC("挑衅"))
            {
                if (CanUseOGCD_NoCombat((uint)PLDActionID.挑衅))
                {
                    IBattleChara? _target = SC.GetSCTarget("挑衅");
                    if (_target != null)
                    {
                        if (_target.DistanceToPlayer() <= 20)
                        {
                            if (Core.Resolve<MemApiSpell>().CheckActionInRangeOrLoS((uint)PLDActionID.挑衅, _target))
                            {
                                await SpellHelper.GetSpell((uint)PLDActionID.挑衅, _target).Cast();
                            }
                        }
                    }
                }
            }
            if (SC.GetSC("退避"))
            {
                if (CanUseOGCD_NoCombat((uint)PLDActionID.调停))
                {
                    IBattleChara? _target = SC.GetSCTarget("调停");
                    if (_target != null)
                    {
                        if (_target.DistanceToPlayer() <= 20)
                        {
                            if (Core.Resolve<MemApiSpell>().CheckActionInRangeOrLoS((uint)PLDActionID.调停, _target))
                            {
                                await SpellHelper.GetSpell((uint)PLDActionID.调停, _target).Cast();
                            }
                        }
                    }
                }
            }
            if (SC.GetSC("干预"))
            {
                if (CanUseOGCD_NoCombat((uint)PLDActionID.调停))
                {
                    IBattleChara? _target = SC.GetSCTarget("调停");
                    if (_target != null)
                    {
                        if (_target.DistanceToPlayer() <= 20)
                        {
                            if (Core.Resolve<MemApiSpell>().CheckActionInRangeOrLoS((uint)PLDActionID.调停, _target))
                            {
                                await SpellHelper.GetSpell((uint)PLDActionID.调停, _target).Cast();
                            }
                        }
                    }
                }
            }
            if (SC.GetSC("保护"))
            {
                if (CanUseOGCD_NoCombat((uint)PLDActionID.调停))
                {
                    IBattleChara? _target = SC.GetSCTarget("调停");
                    if (_target != null)
                    {
                        if (_target.DistanceToPlayer() <= 20)
                        {
                            if (Core.Resolve<MemApiSpell>().CheckActionInRangeOrLoS((uint)PLDActionID.调停, _target))
                            {
                                await SpellHelper.GetSpell((uint)PLDActionID.调停, _target).Cast();
                            }
                        }
                    }
                }
            }
            if (SC.GetSC("深仁厚泽"))
            {
                if (CanUseOGCD_NoCombat((uint)PLDActionID.调停))
                {
                    IBattleChara? _target = SC.GetSCTarget("调停");
                    if (_target != null)
                    {
                        if (_target.DistanceToPlayer() <= 20)
                        {
                            if (Core.Resolve<MemApiSpell>().CheckActionInRangeOrLoS((uint)PLDActionID.调停, _target))
                            {
                                await SpellHelper.GetSpell((uint)PLDActionID.调停, _target).Cast();
                            }
                        }
                    }
                }
            }
            if (SC.GetSC("钢铁信念"))
            {
                if (CanUseOGCD_NoCombat((uint)PLDActionID.钢铁信念))
                {
                    await SpellHelper.GetSpell((uint)PLDActionID.钢铁信念).Cast();
                }
            }
            if (SC.GetSC("冲刺"))
            {
                if (CanUseOGCD_NoCombat((uint)PLDActionID.冲刺))
                {
                    await SpellHelper.GetSpell((uint)PLDActionID.冲刺).Cast();
                }
            }

        }


        public static unsafe void UpdateCombo()
        {
            var battleData = Frost_PLD_BattleData.Instance;
            ActionManager* manager = ActionManager.Instance();
            if (manager == null)
                return;
            ComboDetail comboData = manager->Combo;
            battleData.上次连击技能 = comboData.Action;
            battleData.连击剩余时间 = comboData.Timer;
            battleData.魔法连击剩余时间 = 30f - (float)(battleData.战斗开始时长 - battleData.上次魔法连击时间) / 1000;
            if (battleData.魔法连击剩余时间 < 0f || battleData.魔法连击剩余时间 == 30f)
            {
                battleData.魔法连击剩余时间 = 0f;
            }
        }



    }
}
