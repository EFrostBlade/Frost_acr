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
    internal class 前置_强制突进 : Frost_WAR_ISlotResolver
    {
        public override int Check()
        {
            if (Setting.禁用所有位移技能)
            {
                return -1919;
            }
            if (!QT.GetQt("强制突进"))
            {
                return -1;
            }

            int baseCheck = CanUseAttactOGCD((uint)WARActionID.猛攻, false,false,0,true);
            if (baseCheck == -600)
            {
                return -600;
            }
            if (baseCheck != 0)
            {
                Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("突进不可用,技能使用失败，QT已自动关闭");
                QT.SetQt("强制突进", false);
                return baseCheck;
            }
            float charge = SpellHelper.GetSpell((uint)WARActionID.猛攻).Charges;
            double cooldown = SpellHelper.GetSpell((uint)WARActionID.猛攻).Cooldown.TotalSeconds;
            if (charge < 1 && cooldown > Setting.cd预检测阈值)
            {
                Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("突进cd中，QT已自动关闭");
                QT.SetQt("强制突进", false);
                return -401;
            }
            if (charge < 1 && cooldown > Setting.技能提前时间)
            {
                return -400;
            }
            if(BattleData.当前目标 == null)
            {
                foreach (var keyValuePair in TargetMgr.Instance.EnemysIn20)
                {
                    IBattleChara battleChara = keyValuePair.Value;
                    if (Core.Resolve<MemApiSpell>().CheckActionInRangeOrLoS((uint)WARActionID.猛攻, battleChara))
                    {
                        SpellID = (uint)WARActionID.猛攻;
                        target = battleChara;
                        Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog($"强制突进至{battleChara.Name}");
                        Core.Resolve<MemApiChatMessage>().Toast2($"强制突进至{battleChara.Name}", 1, 2000);
                        return base.Check();
                    }
                }
            }
            if (BattleData.目标距离 > 20)
            {
                return -402;
            }
            if (Core.Resolve<MemApiSpell>().CheckActionInRangeOrLoS((uint)WARActionID.猛攻, BattleData.当前目标))
            {
                SpellID = (uint)WARActionID.猛攻;
                target = BattleData.当前目标;
                Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog($"强制突进至{BattleData.当前目标.Name}");
                Core.Resolve<MemApiChatMessage>().Toast2($"强制突进至{BattleData.当前目标.Name}", 1, 2000);
                return base.Check();
            }

            return -404;
        }
    }
}

