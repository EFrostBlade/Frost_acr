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
    internal class 前置_立刻挑衅 : Frost_WAR_ISlotResolver
    {
        public override int Check()
        {
            if (!QT.GetQt("立刻挑衅"))
            {
                return -1;
            }

            int baseCheck = CanUseRoleAction((uint)WARActionID.挑衅);
            if (baseCheck == -600)
            {
                return -600;
            }
            if (baseCheck != 0)
            {
                Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("挑衅不可用,技能使用失败，QT已自动关闭");
                QT.SetQt("立刻挑衅", false);
                return baseCheck;
            }
            double cooldown = SpellHelper.GetSpell((uint)WARActionID.挑衅).Cooldown.TotalSeconds;
            if (cooldown > Setting.cd预检测阈值)
            {
                Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("挑衅cd中，QT已自动关闭");
                QT.SetQt("立刻挑衅", false);
                return -401;
            }
            if (cooldown > Setting.技能提前时间)
            {
                return -400;
            }
            if (BattleData.当前目标 == null)
            {
                foreach (var keyValuePair in TargetMgr.Instance.EnemysIn25)
                {
                    IBattleChara battleChara = keyValuePair.Value;
                    if (Core.Resolve<MemApiSpell>().CheckActionInRangeOrLoS((uint)WARActionID.挑衅, battleChara))
                    {
                        SpellID = (uint)WARActionID.挑衅;
                        target = battleChara;
                        Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog($"挑衅{battleChara.Name}");
                        Core.Resolve<MemApiChatMessage>().Toast2($"挑衅{battleChara.Name}", 1, 2000);
                        QT.SetQt("立刻挑衅", false);
                        return base.Check();
                    }
                }
            }
            if (BattleData.目标距离 > 25)
            {
                return -402;
            }
            if (Core.Resolve<MemApiSpell>().CheckActionInRangeOrLoS((uint)WARActionID.挑衅, BattleData.当前目标))
            {
                SpellID = (uint)WARActionID.挑衅;
                target = BattleData.当前目标;
                Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog($"挑衅{BattleData.当前目标.Name}");
                Core.Resolve<MemApiChatMessage>().Toast2($"挑衅{BattleData.当前目标.Name}", 1, 2000);
                return base.Check();
            }

            return -404;
        }
    }
}

