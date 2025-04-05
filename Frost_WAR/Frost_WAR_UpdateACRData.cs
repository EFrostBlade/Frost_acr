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
using Frost.Frost_WAR.Frost_WAR_Setting;
using static Frost.Frost_WAR.Frost_WAR_Data.SpellUtils;
using AEAssist.CombatRoutine.Module.AILoop;
using System.Numerics;
using Frost.Frost_WAR.Frost_WAR_Data;
using Frost.Common;
using FFXIVClientStructs.FFXIV.Client.Game.Object;

namespace Frost.Frost_WAR
{
    internal class Frost_WAR_UpdateACRData
    {
        public static void UpdateBattleData()
        {
            // Update Battle data here
            var battleData = Frost_WAR_BattleData.Instance;
            var QT = Frost_WAR_RotationEntry.JobViewWindow;
            battleData.目标距离 = GameObjectExtension.Distance(Core.Me, Core.Me.GetCurrTarget(), DistanceMode.IgnoreAll);
            battleData.目标是否在近战范围内 = battleData.目标距离 <= (float)SettingMgr.GetSetting<GeneralSettings>().AttackRange;
            battleData.自身血量记录.AddLast((Core.Me.CurrentHp, battleData.战斗开始时长));
            if (battleData.自身血量记录.Count > 12000)
            {
                battleData.自身血量记录.RemoveFirst();
            }
            CalculateDamagePerSecond(battleData.自身血量记录, battleData.战斗开始时长);
            battleData.上次连击技能 = Core.Resolve<MemApiSpell>().GetLastComboSpellId();
            UpdateDutyData();
            ulong myID = Core.Me.GameObjectId;
            IBattleChara? myTarget = Core.Me.GetCurrTarget() as IBattleChara;
            battleData.当前目标 = myTarget;
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
            if (TargetMgr.Instance.EnemysIn25 != null)
            {// 在UpdateBattleData方法中
                var enemysIn25Copy = new Dictionary<uint, IBattleChara>(TargetMgr.Instance.EnemysIn25);

                // 创建一个列表用于存储需要从无法拉仇恨的敌人列表中移除的敌人
                var enemiesToRemove = new List<IBattleChara>();


                foreach (var keyValuePair in enemysIn25Copy)
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
                                if (Frost_WAR_DutyData.Instance.无法拉仇恨的敌人列表.Contains(battleChara))
                                {
                                    enemiesToRemove.Add(battleChara);
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
                // 循环结束后一次性从列表中移除
                foreach (var enemy in enemiesToRemove)
                {
                    Frost_WAR_DutyData.Instance.无法拉仇恨的敌人列表.Remove(enemy);
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

            Frost_WAR_BattleData.Instance.自身每秒承伤 = damagePerSecond;
        }
        private static void UpdateBattleState(IBattleChara? myTarget, int enemyIn25, int enemyIn5)
        {
            Frost_WAR_BattleData.Instance.上次战斗状态 = Frost_WAR_BattleData.Instance.当前战斗状态;
            var dutyData = Frost_WAR_DutyData.Instance;

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
            var battleData = Frost_WAR_BattleData.Instance;
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
            var battleData = Frost_WAR_BattleData.Instance;
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
            var battleData = Frost_WAR_BattleData.Instance;
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
            var battleData = Frost_WAR_BattleData.Instance;
            var dutyData = Frost_WAR_DutyData.Instance;
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
            var battleData = Frost_WAR_BattleData.Instance;
            var dutyData = Frost_WAR_DutyData.Instance;
            switch (dutyData.副本人数)
            {
                case 4:
                    if ((enemyIn25 > enemyIn5 + Frost_WAR_DutyData.Instance.无法拉仇恨的敌人列表.Count() && Core.Resolve<MemApiMove>().IsMoving())
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

        public static void UpdateStatusState(IBattleChara? myTarget)
        {
            var battleData = Frost_WAR_BattleData.Instance;
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
            battleData.战场风暴剩余时间 = 0;
            battleData.原初的解放层数 = 0;
            battleData.原初的觉悟剩余时间 = 0;
            battleData.原初的混沌剩余时间 = 0;
            battleData.爆发药剩余时间 = 0;
            battleData.蛮荒崩裂预备剩余时间 = 0;
            battleData.尽毁预备剩余时间 = 0;
            battleData.原初的怒震预备剩余时间 = 0;
            battleData.死斗剩余时间 = 0;
            battleData.狂暴剩余时间 = 0;

            if (SpellHelper.GetSpell((uint)WARActionID.战栗).RecentlyUsed())
            {
                已有buff.Add("战栗");
                自身承伤 *= (1 - 20 / 100.0);
                持有单减 += "战栗" + ",";
            }
            if (SpellHelper.GetSpell((uint)WARActionID.铁壁).RecentlyUsed())
            {
                已有buff.Add("铁壁");
                自身承伤 *= (1 - 20 / 100.0);
                持有单减 += "铁壁" + ",";
            }
            if (SpellHelper.GetSpell((uint)WARActionID.雪仇).RecentlyUsed(3000))
            {
                目标buff.Add("雪仇");
                队伍承伤 *= (1 - 10 / 100.0);
                持有目标减 += "雪仇" + ",";
            }
            if (SpellHelper.GetSpell((uint)WARActionID.亲疏自行).RecentlyUsed(3000))
            {
                目标buff.Add("减速");
                队伍承伤 *= (1 - 15 / 100.0);
                持有目标减 += "减速" + ",";
            }
            if (SpellHelper.GetSpell((uint)WARActionID.原初的血气).RecentlyUsed())
            {
                已有buff.Add("原初的血气");
                自身承伤 *= (1 - 10 / 100.0);
                持有单减 += "原初的血气" + ",";
                已有buff.Add("原初的血潮");
                自身承伤 *= (1 - 10 / 100.0);
                持有单减 += "原初的血潮" + ",";
                已有buff.Add("原初的血烟");
                自身护盾 += 400;
                持有单盾 += "原初的血烟" + ",";
            }
            else if (SpellHelper.GetSpell((uint)WARActionID.原初的直觉).RecentlyUsed())
            {
                已有buff.Add("原初的直觉");
                自身承伤 *= (1 - 10 / 100.0);
                持有单减 += "原初的直觉" + ",";
            }
            if (SpellHelper.GetSpell((uint)WARActionID.戮罪).RecentlyUsed())
            {
                已有buff.Add("戮罪");
                自身承伤 *= (1 - 40 / 100.0);
                持有单减 += "戮罪" + ",";
            }
            else if (SpellHelper.GetSpell((uint)WARActionID.复仇).RecentlyUsed())
            {
                已有buff.Add("复仇");
                自身承伤 *= (1 - 30 / 100.0);
                持有单减 += "复仇" + ",";
            }
            if (SpellHelper.GetSpell((uint)WARActionID.摆脱).RecentlyUsed())
            {
                已有buff.Add("摆脱");
                队伍承伤 *= (1 - 15 / 100.0);
                持有群减 += "摆脱" + ",";
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
                    if (status.StatusId == (uint)WARBuffID.战场风暴)
                    {
                        battleData.战场风暴剩余时间 = Math.Abs(status.RemainingTime);
                    }
                    else if (status.StatusId == (uint)WARBuffID.原初的解放)
                    {
                        battleData.原初的解放层数 = status.StackCount;
                    }
                    else if (status.StatusId == (uint)WARBuffID.原初的觉悟)
                    {
                        battleData.原初的觉悟剩余时间 = Math.Abs(status.RemainingTime);
                    }
                    else if (status.StatusId == (uint)WARBuffID.原初的混沌)
                    {
                        battleData.原初的混沌剩余时间 = Math.Abs(status.RemainingTime);
                    }
                    else if (status.StatusId == (uint)WARBuffID.强化药)
                    {
                        battleData.爆发药剩余时间 = Math.Abs(status.RemainingTime);
                    }
                    else if (status.StatusId == (uint)WARBuffID.蛮荒崩裂预备)
                    {
                        battleData.蛮荒崩裂预备剩余时间 = Math.Abs(status.RemainingTime);
                        if (battleData.蛮荒崩裂预备剩余时间 == 0 && battleData.蛮荒过期提醒 == true)
                        {
                            if (Frost_WAR_Settings.Instance.语音提示)
                            {
                                ChatHelper.SendMessage("/pdr tts 你少打了一个蛮荒，悲痛吧");
                            }
                            battleData.蛮荒过期提醒 = false;
                        }
                    }
                    else if (status.StatusId == (uint)WARBuffID.尽毁预备)
                    {
                        battleData.尽毁预备剩余时间 = Math.Abs(status.RemainingTime);
                    }
                    else if (status.StatusId == (uint)WARBuffID.原初的怒震预备)
                    {
                        battleData.原初的怒震预备剩余时间 = Math.Abs(status.RemainingTime);
                    }
                    else if (status.StatusId == (uint)WARBuffID.死斗)
                    {
                        battleData.死斗剩余时间 = Math.Abs(status.RemainingTime);
                    }
                    else if (status.StatusId == (uint)WARBuffID.狂暴)
                    {
                        battleData.狂暴剩余时间 = Math.Abs(status.RemainingTime);
                    }
                    else if (status.StatusId == (uint)WARBuffID.守护)
                    {
                        是否开盾 = true;
                        battleData.是否开盾 = true;
                    }
                    else if (status.StatusId == (uint)WARBuffID.战技封印1)
                    {
                        是否战技封印 = true;
                        battleData.是否战技封印 = true;
                    }
                    else if (status.StatusId == (uint)WARBuffID.战技封印2)
                    {
                        是否战技封印 = true;
                        battleData.是否战技封印 = true;
                    }
                    else if (status.StatusId == (uint)WARBuffID.能力封印1)
                    {
                        是否能力封印 = true;
                        battleData.是否能力封印 = true;
                    }
                    else if (status.StatusId == (uint)WARBuffID.能力封印2)
                    {
                        是否能力封印 = true;
                        battleData.是否能力封印 = true;
                    }
                    else if (status.StatusId == (uint)WARBuffID.能力封印3)
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
            var battleData = Frost_WAR_BattleData.Instance;
            var QT = Frost_WAR_RotationEntry.JobViewWindow;
            if (QT.GetQt("爆发药") && Frost_WAR_Settings.Instance.自动保留尽毁 == true)
            {
                if (QT.GetQt("爆发药不对团辅") && battleData.已打解放次数 > 0)
                {

                    if ((float)SpellsDefine.Potion.GetSpell().Cooldown.TotalSeconds < 60f)
                    {
                        if (!battleData.已开保留尽毁)
                        {
                            Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("爆发药即将转好，开始保留资源");
                            QT.SetQt("保留尽毁", true);
                            battleData.已开保留尽毁 = true;
                        }
                    }
                }
                else
                {
                    if ((float)SpellsDefine.Potion.GetSpell().Cooldown.TotalSeconds == 0f
                        && battleData.战斗开始分钟数 % 2 == 1
                        && (float)SpellHelper.GetSpell((uint)WARActionID.原初的解放).Cooldown.TotalSeconds < 3f)
                    {
                        if (!battleData.已开保留尽毁)
                        {
                            Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("爆发药即将转好，开始保留资源");
                            QT.SetQt("保留尽毁", true);
                            battleData.已开保留尽毁 = true;
                        }
                    }
                }
            }
            if (QT.GetQt("停手"))
            {
                //Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("切换至停手");
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
                    //tempSlotResolver.Add(new SlotResolverData(new Frost_WAR_泄资源_Defence_SlotResolvers(), SlotMode.OffGcd));
                    //tempSlotResolver.Add(new SlotResolverData(new Frost_WAR_泄资源_OGCD_SlotResolvers(), SlotMode.OffGcd));
                    //tempSlotResolver.Add(new SlotResolverData(new Frost_WAR_泄资源_GCD_SlotResolvers(), SlotMode.Gcd));
                    //Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("切换至泄资源状态");

                    battleData.ResolverState = 1;
                }
                //拉怪途中逻辑
                else if (battleData.当前战斗状态 == 战斗状态.四人本拉怪途中)
                {
                    //tempSlotResolver.Add(new SlotResolverData(new Frost_WAR_拉怪_Defence_SlotResolvers(), SlotMode.OffGcd));
                    //tempSlotResolver.Add(new SlotResolverData(new Frost_WAR_拉怪_OGCD_SlotResolvers(), SlotMode.OffGcd));
                    //tempSlotResolver.Add(new SlotResolverData(new Frost_WAR_拉怪_GCD_SlotResolvers(), SlotMode.Gcd));
                    battleData.ResolverState = 2;
                }
                // 小怪战斗即将结束逻辑
                else if (battleData.当前战斗状态 == 战斗状态.四人本小怪战斗即将结束
                    || battleData.当前战斗状态 == 战斗状态.八人本小怪战斗即将结束
                    || battleData.当前战斗状态 == 战斗状态.团本小怪战斗即将结束)
                {
                    //tempSlotResolver.Add(new SlotResolverData(new Frost_WAR_小怪战斗即将结束_Defence_SlotResolvers(), SlotMode.OffGcd));
                    //tempSlotResolver.Add(new SlotResolverData(new Frost_WAR_小怪战斗即将结束_OGCD_SlotResolvers(), SlotMode.OffGcd));
                    //tempSlotResolver.Add(new SlotResolverData(new Frost_WAR_小怪战斗即将结束_GCD_SlotResolvers(), SlotMode.Gcd));
                    //Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("切换至小怪战斗即将结束状态");
                    battleData.ResolverState = 4;
                }
                //小怪战斗中逻辑
                else if (battleData.当前战斗状态 == 战斗状态.四人本小怪战斗中
                    || battleData.当前战斗状态 == 战斗状态.八人本小怪战斗中
                    || battleData.当前战斗状态 == 战斗状态.野外小怪战斗中
                    || battleData.当前战斗状态 == 战斗状态.野外小怪战斗即将结束
                    || battleData.当前战斗状态 == 战斗状态.团本小怪战斗中)
                {
                    //tempSlotResolver.Add(new SlotResolverData(new Frost_WAR_小怪战斗中_Defence_SlotResolvers(), SlotMode.OffGcd));
                    //tempSlotResolver.Add(new SlotResolverData(new Frost_WAR_小怪战斗中_OGCD_SlotResolvers(), SlotMode.OffGcd));
                    //tempSlotResolver.Add(new SlotResolverData(new Frost_WAR_小怪战斗中_GCD_SlotResolvers(), SlotMode.Gcd));
                    battleData.ResolverState = 3;
                }
                //BOSS战斗中逻辑
                else if (battleData.当前战斗状态 == 战斗状态.四人本BOSS战斗中
                    || battleData.当前战斗状态 == 战斗状态.八人本BOSS战斗中
                    || battleData.当前战斗状态 == 战斗状态.团本BOSS战斗中
                    || battleData.当前战斗状态 == 战斗状态.野外BOSS战斗中
                    || battleData.当前战斗状态 == 战斗状态.高难本BOSS战斗中)
                {
                    //tempSlotResolver.Add(new SlotResolverData(new Frost_WAR_BOSS战斗中_Defence_SlotResolvers(), SlotMode.OffGcd));
                    //tempSlotResolver.Add(new SlotResolverData(new Frost_WAR_BOSS战斗中_OGCD_SlotResolvers(), SlotMode.OffGcd));
                    //tempSlotResolver.Add(new SlotResolverData(new Frost_WAR_BOSS战斗中_GCD_SlotResolvers(), SlotMode.Gcd));
                    //Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("切换至BOSS战斗中状态");
                    battleData.ResolverState = 5;
                }
            }
        }
        public static void UpdateDutyData(bool log = false)
        {
            var dutyData = Frost_WAR_DutyData.Instance;
            DutyInfo dutyInfo = new DutyInfo();
            dutyData.是否在副本中 = dutyInfo.InDuty;
            dutyData.副本人数 = dutyInfo.MemberNum;
        }
        public static async Task OnNoCombat()
        {
            await QtSpell();
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
                                if (CanUseOGCD_NoCombat((uint)WARActionID.重劈))
                                {
                                    await SpellHelper.GetSpell((uint)WARActionID.重劈, SpellTargetType.Target).Cast();
                                }
                            }
                            else if (GameObjectExtension.Distance(Core.Me, Core.Me.GetCurrTarget(), DistanceMode.IgnoreAll) <= 20)
                            {
                                if (CanUseOGCD_NoCombat((uint)WARActionID.飞斧))
                                {
                                    if (Core.Resolve<MemApiSpell>().CheckActionInRangeOrLoS((uint)WARActionID.飞斧, target))
                                    {
                                        await SpellHelper.GetSpell((uint)WARActionID.飞斧, SpellTargetType.Target).Cast();
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public static async Task QtSpell()
        {
            var battleData = Frost_WAR_BattleData.Instance;
            var QT = Frost_WAR_RotationEntry.JobViewWindow;

            if (Frost_WAR_Settings.Instance.启用qt控制盾姿)
            {
                bool isShieldActive = GameObjectExtension.HasAura(Core.Me, (uint)WARBuffID.守护);
                bool shouldActivateShield = Frost_WAR_RotationEntry.JobViewWindow.GetQt("盾姿");
                bool isShieldColldown = SpellHelper.GetSpell((uint)WARActionID.守护).Cooldown.TotalSeconds == 0 && SpellHelper.GetSpell((uint)WARActionID.解除守护).Cooldown.TotalSeconds == 0;

                if (isShieldColldown && shouldActivateShield && !isShieldActive && !SpellHelper.GetSpell((uint)WARActionID.守护).RecentlyUsed() && !SpellHelper.GetSpell((uint)WARActionID.解除守护).RecentlyUsed())
                {
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("战斗外开盾");
                    await SpellHelper.GetSpell((uint)WARActionID.守护).Cast();
                }
                else if (isShieldColldown && !shouldActivateShield && isShieldActive && !SpellHelper.GetSpell((uint)WARActionID.解除守护).RecentlyUsed() && !SpellHelper.GetSpell((uint)WARActionID.守护).RecentlyUsed())
                {
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("战斗外关盾");
                    await SpellHelper.GetSpell((uint)WARActionID.解除守护).Cast();
                }
            }
            if (Core.Resolve<MemApiCondition>().IsBoundByDuty()
                && Core.Resolve<MemApiMove>().IsMoving()
                && Frost_WAR_Settings.Instance.自动疾跑)
            {
                if (Frost_WAR_DutyData.Instance.副本人数 == 4 || Frost_WAR_DutyData.Instance.副本人数 == 24)
                {
                    if (DateTime.Now - Frost_WAR_DutyData.Instance.上次疾跑时间 > TimeSpan.FromSeconds(60))
                    {
                        if (CanUseOGCD_NoCombat((uint)WARActionID.冲刺))
                        {
                            await SpellHelper.GetSpell((uint)WARActionID.冲刺).Cast();
                            Frost_WAR_DutyData.Instance.上次疾跑时间 = DateTime.Now;
                        }
                    }
                }
            }
            if (Core.Resolve<MemApiCondition>().IsBoundByDuty()
                && Core.Resolve<MemApiMove>().IsMoving()
                && Frost_WAR_Settings.Instance.自动拉怪突进)
            {
                if (Frost_WAR_DutyData.Instance.副本人数 == 4 || Frost_WAR_DutyData.Instance.副本人数 == 24)
                {
                    if (TargetMgr.Instance.EnemysIn20 != null)
                    {
                        // 创建一个EnemysIn20的副本，避免在遍历时集合被修改导致异常
                        var enemyList = new Dictionary<uint, IBattleChara>(TargetMgr.Instance.EnemysIn20);
                        foreach (var keyValuePair in enemyList)
                        {
                            if (CanUseOGCD_NoCombat((uint)WARActionID.猛攻))
                            {
                                IBattleChara enemy = keyValuePair.Value;
                                if (GameObjectExtension.Distance(Core.Me, enemy, DistanceMode.IgnoreAll) > 12
                                    && Core.Resolve<MemApiSpell>().CheckActionInRangeOrLoS((uint)WARActionID.猛攻, enemy))
                                {
                                    // 计算目标是否在当前面向范围内
                                    var directionToTarget = enemy.Position - Core.Me.Position;
                                    var forwardDirection = new Vector3((float)Math.Sin(Core.Me.Rotation), 0, (float)Math.Cos(Core.Me.Rotation));
                                    var angle = Math.Acos(Vector3.Dot(Vector3.Normalize(forwardDirection), Vector3.Normalize(directionToTarget))) * (180.0 / Math.PI);

                                    if (angle <= 40)
                                    {
                                        Slot slot = new Slot();
                                        slot.Add(new Spell((uint)WARActionID.猛攻, enemy));
                                        await slot.Run(AI.Instance.BattleData, isNextSlot: false);
                                        Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog($"自动突进{enemy.Name}拉怪");
                                        Core.Resolve<MemApiChatMessage>().Toast2($"自动突进{enemy.Name}拉怪", 1, 2000);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            // 死斗逻辑
            if (QT.GetQt("立刻死斗"))
            {
                if (CanUseOGCD_NoCombat((uint)WARActionID.死斗))
                {
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("死斗");
                    await SpellHelper.GetSpell((uint)WARActionID.死斗).Cast();
                    QT.SetQt("立刻死斗", false);
                }
                else if (SpellHelper.GetSpell((uint)WARActionID.死斗).Cooldown.TotalSeconds > 3f || !SpellHelper.GetSpell((uint)WARActionID.死斗).IsUnlock())
                {
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("死斗CD中,技能使用失败，QT已自动关闭");
                    QT.SetQt("立刻死斗", false);
                }
            }

            // 血气逻辑
            if (QT.GetQt("立刻血气"))
            {
                if (CanUseOGCD_NoCombat((uint)WARActionID.原初的血气))
                {
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("原初的血气");
                    await SpellHelper.GetSpell((uint)WARActionID.原初的血气).Cast();
                    QT.SetQt("立刻血气", false);
                }
                else if (SpellHelper.GetSpell((uint)WARActionID.原初的血气).Cooldown.TotalSeconds > 3f || !SpellHelper.GetSpell((uint)WARActionID.原初的血气).IsUnlock())
                {
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("原初的血气CD中,技能使用失败，QT已自动关闭");
                    QT.SetQt("立刻血气", false);
                }

                if (Core.Me.Level < 82
                    && CanUseOGCD_NoCombat((uint)WARActionID.原初的直觉))
                {
                    await SpellHelper.GetSpell((uint)WARActionID.原初的直觉).Cast();
                    QT.SetQt("立刻血气", false);
                }
                else if (SpellHelper.GetSpell((uint)WARActionID.原初的直觉).Cooldown.TotalSeconds > 3f
                    || !SpellHelper.GetSpell((uint)WARActionID.原初的直觉).IsUnlock())
                {
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("原初的直觉CD中,技能使用失败，QT已自动关闭");
                    QT.SetQt("立刻血气", false);
                }
            }

            // 勇猛逻辑
            if (QT.GetQt("立刻勇猛"))
            {
                if (CanUseOGCD_NoCombat((uint)WARActionID.原初的勇猛))
                {
                    IBattleChara? anotherTank = PartyHelper.GetAnotherTank(Core.Me);
                    if (anotherTank != null)
                    {
                        if (Core.Resolve<MemApiSpell>().CheckActionInRangeOrLoS((uint)WARActionID.原初的勇猛, anotherTank))
                        {
                            Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("原初的勇猛");
                            Slot slot = new Slot();
                            slot.Add(new Spell((uint)WARActionID.原初的勇猛, anotherTank));
                            await slot.Run(AI.Instance.BattleData, isNextSlot: false);
                            QT.SetQt("立刻勇猛", false);
                        }
                    }
                    else
                    {
                        IBattleChara lowestHpTarget = null;
                        float lowestHpPercent = 1;

                        foreach (IBattleChara partyer in PartyHelper.CastableAlliesWithin30)
                        {
                            if (partyer.CurrentHpPercent() < lowestHpPercent)
                            {
                                lowestHpPercent = partyer.CurrentHpPercent();
                                lowestHpTarget = partyer;
                            }
                        }

                        if (lowestHpTarget != null && lowestHpTarget != Core.Me)
                        {
                            if (Core.Resolve<MemApiSpell>().CheckActionInRangeOrLoS((uint)WARActionID.原初的勇猛, lowestHpTarget))
                            {
                                Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("原初的勇猛");
                                Slot slot = new Slot();
                                slot.Add(new Spell((uint)WARActionID.原初的勇猛, lowestHpTarget));
                                await slot.Run(AI.Instance.BattleData, isNextSlot: false);
                                QT.SetQt("立刻勇猛", false);

                            }
                        }
                    }
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("无勇猛目标，qt已自动关闭");
                    QT.SetQt("立刻勇猛", false);
                }
                else if (SpellHelper.GetSpell((uint)WARActionID.原初的勇猛).Cooldown.TotalSeconds > 3f || !SpellHelper.GetSpell((uint)WARActionID.原初的勇猛).IsUnlock())
                {
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("原初的勇猛CD中,技能使用失败，QT已自动关闭");
                    QT.SetQt("立刻勇猛", false);
                }

                Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("无勇猛目标，qt已自动关闭");
                QT.SetQt("立刻勇猛", false);
            }

            // 勇猛远敏逻辑
            if (QT.GetQt("立刻勇猛远敏"))
            {
                if (CanUseOGCD_NoCombat((uint)WARActionID.原初的勇猛))
                {

                    foreach (IBattleChara rangeds in PartyHelper.CastableRangeds)
                    {
                        if (GameObjectExtension.Distance(Core.Me, rangeds, DistanceMode.IgnoreAll) > 30)
                        {
                            continue;
                        }
                        if (rangeds.CurrentJob() == Jobs.Bard || rangeds.CurrentJob() == Jobs.Dancer || rangeds.CurrentJob() == Jobs.Machinist)
                        {
                            if (Core.Resolve<MemApiSpell>().CheckActionInRangeOrLoS((uint)WARActionID.原初的勇猛, rangeds))
                            {
                                Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog($"勇猛{rangeds.Name}");
                                Core.Resolve<MemApiChatMessage>().Toast2($"勇猛{rangeds.Name}", 1, 2000);
                                Slot slot = new Slot();
                                slot.Add(new Spell((uint)WARActionID.原初的勇猛, rangeds));
                                await slot.Run(AI.Instance.BattleData, isNextSlot: false);
                                QT.SetQt("立刻勇猛远敏", false);
                            }
                        }
                    }
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("无勇猛目标，qt已自动关闭");
                    QT.SetQt("立刻勇猛远敏", false);
                }
                else if (SpellHelper.GetSpell((uint)WARActionID.原初的勇猛).Cooldown.TotalSeconds > 3f || !SpellHelper.GetSpell((uint)WARActionID.原初的勇猛).IsUnlock())
                {
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("原初的勇猛CD中,技能使用失败，QT已自动关闭");
                    QT.SetQt("立刻勇猛远敏", false);
                }

                Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("无勇猛目标，qt已自动关闭");
                QT.SetQt("立刻勇猛远敏", false);
            }
            // 勇猛法系逻辑
            if (QT.GetQt("立刻勇猛法系"))
            {
                if (CanUseOGCD_NoCombat((uint)WARActionID.原初的勇猛))
                {

                    foreach (IBattleChara rangeds in PartyHelper.CastableRangeds)
                    {
                        if (GameObjectExtension.Distance(Core.Me, rangeds, DistanceMode.IgnoreAll) > 30)
                        {
                            continue;
                        }
                        if (rangeds.CurrentJob() == Jobs.BlackMage || rangeds.CurrentJob() == Jobs.RedMage || rangeds.CurrentJob() == Jobs.Summoner)
                        {
                            if (Core.Resolve<MemApiSpell>().CheckActionInRangeOrLoS((uint)WARActionID.原初的勇猛, rangeds))
                            {
                                Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog($"勇猛{rangeds.Name}");
                                Core.Resolve<MemApiChatMessage>().Toast2($"勇猛{rangeds.Name}", 1, 2000);
                                Slot slot = new Slot();
                                slot.Add(new Spell((uint)WARActionID.原初的勇猛, rangeds));
                                await slot.Run(AI.Instance.BattleData, isNextSlot: false);
                                QT.SetQt("立刻勇猛法系", false);
                            }
                        }
                    }
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("无勇猛目标，qt已自动关闭");
                    QT.SetQt("立刻勇猛法系", false);
                }
                else if (SpellHelper.GetSpell((uint)WARActionID.原初的勇猛).Cooldown.TotalSeconds > 3f || !SpellHelper.GetSpell((uint)WARActionID.原初的勇猛).IsUnlock())
                {
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("原初的勇猛CD中,技能使用失败，QT已自动关闭");
                    QT.SetQt("立刻勇猛法系", false);
                }

                Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("无勇猛目标，qt已自动关闭");
                QT.SetQt("立刻勇猛法系", false);
            }

            // 雪仇逻辑
            if (QT.GetQt("立刻雪仇"))
            {
                if (CanUseOGCD_NoCombat((uint)WARActionID.雪仇))
                {
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("雪仇");
                    await SpellHelper.GetSpell((uint)WARActionID.雪仇).Cast();
                    QT.SetQt("立刻雪仇", false);
                }
                else if (SpellHelper.GetSpell((uint)WARActionID.雪仇).Cooldown.TotalSeconds > 3f || !SpellHelper.GetSpell((uint)WARActionID.雪仇).IsUnlock())
                {
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("雪仇CD中,技能使用失败，QT已自动关闭");
                    QT.SetQt("立刻雪仇", false);
                }
            }

            // 铁壁逻辑
            if (QT.GetQt("立刻铁壁"))
            {
                if (CanUseOGCD_NoCombat((uint)WARActionID.铁壁))
                {
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("铁壁");
                    await SpellHelper.GetSpell((uint)WARActionID.铁壁).Cast();
                    QT.SetQt("立刻铁壁", false);
                }
                else if (SpellHelper.GetSpell((uint)WARActionID.铁壁).Cooldown.TotalSeconds > 3f || !SpellHelper.GetSpell((uint)WARActionID.铁壁).IsUnlock())
                {
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("铁壁CD中,技能使用失败，QT已自动关闭");
                    QT.SetQt("立刻铁壁", false);
                }
            }

            // 战栗逻辑
            if (QT.GetQt("立刻战栗"))
            {
                if (CanUseOGCD_NoCombat((uint)WARActionID.战栗))
                {
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("战栗");
                    await SpellHelper.GetSpell((uint)WARActionID.战栗).Cast();
                    QT.SetQt("立刻战栗", false);
                }
                else if (SpellHelper.GetSpell((uint)WARActionID.战栗).Cooldown.TotalSeconds > 3f || !SpellHelper.GetSpell((uint)WARActionID.战栗).IsUnlock())
                {
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("战栗CD中,技能使用失败，QT已自动关闭");
                    QT.SetQt("立刻战栗", false);
                }
            }

            // 泰然逻辑
            if (QT.GetQt("立刻泰然"))
            {
                if (CanUseOGCD_NoCombat((uint)WARActionID.泰然自若))
                {
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("泰然自若");
                    await SpellHelper.GetSpell((uint)WARActionID.泰然自若).Cast();
                    QT.SetQt("立刻泰然", false);
                }
                else if (SpellHelper.GetSpell((uint)WARActionID.泰然自若).Cooldown.TotalSeconds > 3f || !SpellHelper.GetSpell((uint)WARActionID.泰然自若).IsUnlock())
                {
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("泰然自若CD中,技能使用失败，QT已自动关闭");
                    QT.SetQt("立刻泰然", false);
                }
            }
            if (QT.GetQt("立刻戮罪"))
            {
                if (CanUseOGCD_NoCombat((uint)WARActionID.戮罪))
                {
                    await SpellHelper.GetSpell((uint)WARActionID.戮罪).Cast();
                    QT.SetQt("立刻戮罪", false);
                }
                else if (SpellHelper.GetSpell((uint)WARActionID.戮罪).Cooldown.TotalSeconds > 3f || !SpellHelper.GetSpell((uint)WARActionID.戮罪).IsUnlock())
                {
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("戮罪CD中,技能使用失败，QT已自动关闭");
                    QT.SetQt("立刻戮罪", false);
                }
                if (Core.Me.Level < 92 && CanUseOGCD_NoCombat((uint)WARActionID.复仇))
                {
                    await SpellHelper.GetSpell((uint)WARActionID.复仇).Cast();
                    QT.SetQt("立刻戮罪", false);
                }
                else if (SpellHelper.GetSpell((uint)WARActionID.复仇).Cooldown.TotalSeconds > 3f || !SpellHelper.GetSpell((uint)WARActionID.复仇).IsUnlock())
                {
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("复仇CD中,技能使用失败，QT已自动关闭");
                    QT.SetQt("立刻戮罪", false);
                }
            }
            if (QT.GetQt("立刻摆脱"))
            {
                if (CanUseOGCD_NoCombat((uint)WARActionID.摆脱))
                {
                    await SpellHelper.GetSpell((uint)WARActionID.摆脱).Cast();
                    QT.SetQt("立刻摆脱", false);
                }
                else if (SpellHelper.GetSpell((uint)WARActionID.摆脱).Cooldown.TotalSeconds > 3f || !SpellHelper.GetSpell((uint)WARActionID.摆脱).IsUnlock())
                {
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("摆脱CD中,技能使用失败，QT已自动关闭");
                    QT.SetQt("立刻摆脱", false);
                }
            }
            if (QT.GetQt("立刻防击退"))
            {
                if (CanUseOGCD_NoCombat((uint)WARActionID.亲疏自行))
                {
                    await SpellHelper.GetSpell((uint)WARActionID.亲疏自行).Cast();
                    QT.SetQt("立刻防击退", false);
                }
                else if (SpellHelper.GetSpell((uint)WARActionID.亲疏自行).Cooldown.TotalSeconds > 3f || !SpellHelper.GetSpell((uint)WARActionID.亲疏自行).IsUnlock())
                {
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("亲疏自行CD中,技能使用失败，QT已自动关闭");
                    QT.SetQt("立刻防击退", false);
                }
            }
            if (QT.GetQt("立刻突进"))
            {
                if (CanUseOGCD_NoCombat((uint)WARActionID.猛攻))
                {
                    await SpellHelper.GetSpell((uint)WARActionID.猛攻).Cast();
                    QT.SetQt("立刻突进", false);
                }
                else if (SpellHelper.GetSpell((uint)WARActionID.猛攻).Charges < 1f || !SpellHelper.GetSpell((uint)WARActionID.猛攻).IsUnlock())
                {
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("猛攻CD中,技能使用失败，QT已自动关闭");
                    QT.SetQt("立刻突进", false);
                }
            }
            if (QT.GetQt("强制突进"))
            {
                if (CanUseOGCD_NoCombat((uint)WARActionID.猛攻))
                {
                    await SpellHelper.GetSpell((uint)WARActionID.猛攻).Cast();
                    QT.SetQt("强制突进", false);
                }
                else if (SpellHelper.GetSpell((uint)WARActionID.猛攻).Charges < 1f || !SpellHelper.GetSpell((uint)WARActionID.猛攻).IsUnlock())
                {
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("猛攻CD中,技能使用失败，QT已自动关闭");
                    QT.SetQt("强制突进", false);
                }
            }
            if (QT.GetQt("立刻冲刺"))
            {
                if (CanUseOGCD_NoCombat((uint)WARActionID.冲刺))
                {
                    await SpellHelper.GetSpell((uint)WARActionID.冲刺).Cast();
                    QT.SetQt("立刻冲刺", false);
                }
                else if (SpellHelper.GetSpell((uint)WARActionID.冲刺).Cooldown.TotalSeconds > 3f || !SpellHelper.GetSpell((uint)WARActionID.冲刺).IsUnlock())
                {
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("冲刺CD中,技能使用失败，QT已自动关闭");
                    QT.SetQt("立刻冲刺", false);
                }
            }
            if (QT.GetQt("立刻爆发药"))
            {
                if (Spell.CreatePotion().IsUnlock() && SpellsDefine.Potion.GetSpell().Cooldown.TotalSeconds == 0)
                {
                    await Spell.CreatePotion().Cast();
                    QT.SetQt("立刻爆发药", false);
                }
                else if (SpellsDefine.Potion.GetSpell().Cooldown.TotalSeconds > 3f)
                {
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("爆发药CD中,使用失败，QT已自动关闭");
                    QT.SetQt("立刻爆发药", false);
                }
            }

            if (QT.GetQt("立刻退避"))
            {
                Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("战斗外不应该使用退避，QT已自动关闭");
                QT.SetQt("立刻退避", false);
            }

            if (QT.GetQt("立刻挑衅"))
            {
                if (CanUseOGCD_NoCombat((uint)WARActionID.挑衅))
                {
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("挑衅");
                    await SpellHelper.GetSpell((uint)WARActionID.挑衅).Cast();
                    QT.SetQt("立刻挑衅", false);
                }
                else if (SpellHelper.GetSpell((uint)WARActionID.挑衅).Cooldown.TotalSeconds > 3f || !SpellHelper.GetSpell((uint)WARActionID.挑衅).IsUnlock())
                {
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("挑衅CD中,技能使用失败，QT已自动关闭");
                    QT.SetQt("立刻挑衅", false);
                }
                Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("挑衅无目标，QT已自动关闭");
                QT.SetQt("立刻挑衅", false);
            }
        }

    }
}
