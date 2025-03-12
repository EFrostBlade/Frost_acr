using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.CombatRoutine.View.JobView;
using AEAssist.Helper;
using AEAssist.MemoryApi;
using Dalamud.Game.ClientState.Objects.Types;
using Frost.Frost_WAR.Frost_WAR_Data;
using Frost.Frost_WAR.Frost_WAR_Setting;

namespace Frost.Frost_WAR.Frost_WAR_SlotResolvers
{
    internal abstract class Frost_WAR_ISlotResolver : ISlotResolver
    {
        public SlotMode SlotMode => (SlotMode)SlotMode.Always;
        protected uint SpellID = 0;
        protected IBattleChara? target = null;
        protected Spell? spell = null;

        protected Frost_WAR_BattleData BattleData => Frost_WAR_BattleData.Instance;
        protected JobViewWindow QT => Frost_WAR_RotationEntry.JobViewWindow;
        protected Frost_WAR_Settings Setting => Frost_WAR_Settings.Instance;


        /// <summary>
        ///
        /// </summary>
        /// <param name="spellId"></param>
        /// <param name="是否近战"></param>
        /// <param name="是否aoe"></param>
        /// <returns> -501:技能状态不正确 -502:时间轴锁定 -503:战技封印 -504:等级不足 -505:目标不在近战范围内 -506:禁用aoe -507:不可打aoe -508:红斩时间不足</returns>
        public static int CanUseGCD(uint spellId, int 需要战场风暴时间 = 0, bool 是否近战 = true, bool 是否aoe = false)
        {
            var battleData = Frost_WAR_BattleData.Instance;
            var QT = Frost_WAR_RotationEntry.JobViewWindow;

            if (Core.Resolve<MemApiSpell>().GetActionState(spellId) == 572)  //释放条件未满足
            {
                return -572;
            }
            if (Core.Resolve<MemApiSpell>().GetActionState(spellId) == 573) //尚未学习技能
            {
                return -573;
            }

            if (Core.Resolve<MemApiSpell>().GetActionState(spellId) == 579) //状态限制
            {
                return -579;
            }



            if (AI.Instance.BattleData.LockSpells.Contains(spellId))
            {
                return -502;
            }
            if (battleData.是否战技封印)
            {
                return -503;
            }
            if (!Core.Resolve<MemApiSpell>().IsLevelEnough(spellId))
            {
                return -504;
            }
            if (GCDHelper.GetGCDCooldown() > Frost_WAR_Settings.Instance.技能提前时间)
            {
                return -600;
            }
            if (是否近战 && !battleData.目标是否在近战范围内)
            {
                return -505;
            }
            if (是否aoe && QT.GetQt("禁用aoe"))
            {
                return -506;
            }
            if (是否aoe && !battleData.是否可打aoe)
            {
                return -507;
            }
            if (需要战场风暴时间 > 0 && battleData.战场风暴剩余时间 < 需要战场风暴时间)
            {
                return -508;
            }
            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spellId"></param>
        /// <param name="是否近战"></param>
        /// <param name="是否aoe"></param>
        /// <returns>-501:技能状态不正确 -502:时间轴锁定 -503:能力封印 -504:等级不足 -505:目标不在近战范围内 -506:禁用aoe -507:不可打aoe -508:红斩时间不足</returns>
        public static int CanUseAttactOGCD(uint spellId, bool 是否近战 = true, bool 是否aoe = false, int 需要战场风暴时间 = 0, bool 强制插入 = false)
        {
            var battleData = Frost_WAR_BattleData.Instance;
            var QT = Frost_WAR_RotationEntry.JobViewWindow;

            if (Core.Resolve<MemApiSpell>().GetActionState(spellId) == 572)  //释放条件未满足
            {
                return -572;
            }
            if (Core.Resolve<MemApiSpell>().GetActionState(spellId) == 573) //尚未学习技能
            {
                return -573;
            }

            if (Core.Resolve<MemApiSpell>().GetActionState(spellId) == 579) //状态限制
            {
                return -579;
            }


            if (AI.Instance.BattleData.LockSpells.Contains(spellId))
            {
                return -502;
            }
            if (battleData.是否能力封印)
            {
                return -503;
            }
            if (!Core.Resolve<MemApiSpell>().IsLevelEnough(spellId))
            {
                return -504;
            }
            if (GCDHelper.GetGCDCooldown() != 0 && GCDHelper.GetGCDCooldown() < 600 && !强制插入)
            {
                return -600;
            }
            if (是否近战 && !battleData.目标是否在近战范围内)
            {
                return -505;
            }
            if (是否aoe && QT.GetQt("禁用aoe"))
            {
                return -506;
            }
            if (是否aoe && !battleData.是否可打aoe)
            {
                return -507;
            }
            if (需要战场风暴时间 > 0 && battleData.战场风暴剩余时间 < 需要战场风暴时间)
            {
                return -508;
            }
            return 0;
        }
        /// <summary>
        ///
        /// </summary>
        /// <param name="spellId"></param>
        /// <returns> -501:技能状态不正确 -502:时间轴锁定 -503:能力封印 -504:等级不足</returns>
        public static int CanUseDefenceOGCD(uint spellId, bool 是否近战 = false, bool 强制插入 = false)
        {
            var battleData = Frost_WAR_BattleData.Instance;
            var QT = Frost_WAR_RotationEntry.JobViewWindow;

            if (Core.Resolve<MemApiSpell>().GetActionState(spellId) == 572)  //释放条件未满足
            {
                return -572;
            }
            if (Core.Resolve<MemApiSpell>().GetActionState(spellId) == 573) //尚未学习技能
            {
                return -573;
            }

            if (Core.Resolve<MemApiSpell>().GetActionState(spellId) == 579) //状态限制
            {
                return -579;
            }

            if (AI.Instance.BattleData.LockSpells.Contains(spellId))
            {
                return -502;
            }
            if (battleData.是否能力封印)
            {
                return -503;
            }
            if (!Core.Resolve<MemApiSpell>().IsLevelEnough(spellId))
            {
                return -504;
            }
            if (GCDHelper.GetGCDCooldown() != 0 && GCDHelper.GetGCDCooldown() < 600 && !强制插入)
            {
                return -600;
            }
            return 0;
        }
        /// <summary>
        ///
        /// </summary>
        /// <param name="spellId"></param>
        /// <param name="是否近战"></param>
        /// <returns> -501:技能状态不正确 -502:时间轴锁定 -503:战技封印 -505:目标不在近战范围内</returns>
        public static int CanUseRoleAction(uint spellId, bool 是否近战 = false, bool 强制插入 = false)
        {
            var battleData = Frost_WAR_BattleData.Instance;
            if (Core.Resolve<MemApiSpell>().GetActionState(spellId) == 572)  //释放条件未满足
            {
                return -572;
            }
            if (Core.Resolve<MemApiSpell>().GetActionState(spellId) == 573) //尚未学习技能
            {
                return -573;
            }

            if (Core.Resolve<MemApiSpell>().GetActionState(spellId) == 579) //状态限制
            {
                return -579;
            }
            if (AI.Instance.BattleData.LockSpells.Contains(spellId))
            {
                return -502;
            }
            if (battleData.是否能力封印)
            {
                return -503;
            }
            if (GCDHelper.GetGCDCooldown() != 0 && GCDHelper.GetGCDCooldown() < 600 && !强制插入)
            {
                return -600;
            }
            if (是否近战 && !battleData.目标是否在近战范围内)
            {
                return -505;
            }
            return 0;
        }

        public virtual int Check()
        {
            if (SpellID != 0 && target != null)
            {
                spell = (new Spell((uint)SpellID, target));
                if (spell.IsReadyWithCanCast())
                {
                    //Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog($"技能{spell.Name}," +
                    //    $"目标{target.Name}在AE的检测通过。");
                    return 0;
                }
                else
                {
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog($"技能{spell.Name}," +
                        $"目标{target.Name}在AE的检测不通过。这一定是AE的问题。");
                    return 0;
                }
            }
            return -500;
        }

        public virtual void Build(Slot slot)
        {
            if (spell == null)
            {
                //Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog($"NULL触发build");
            }
            else
            {
                //Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog($"{spell.Name}触发build");

            }
            if (spell != null)
            {
                //Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog($"释放{spell.Name}");
                slot.Add(spell);
                BattleData.isChecking = false;
            }
        }
    }
}
