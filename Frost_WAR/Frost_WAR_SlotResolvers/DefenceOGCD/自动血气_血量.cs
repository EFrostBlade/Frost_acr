using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Extension;
using AEAssist.Helper;
using AEAssist.MemoryApi;
using Dalamud.Game.ClientState.Objects.Types;
using Frost.Frost_WAR.Frost_WAR_Data;

namespace Frost.Frost_WAR.Frost_WAR_SlotResolvers
{
    internal class 自动血气_血量 : Frost_WAR_ISlotResolver
    {
        public override int Check()
        {
            if (!Setting.自动减伤)
            {
                return -1;
            }

            float hpPercent = GameObjectExtension.CurrentHpPercent(Core.Me);
            if (hpPercent > 0.7f)
            {
                return -2;
            }
            if (BattleData.死斗剩余时间 > 1f)
            {
                return -3;
            }

            if (Core.Me.Level >= 82)
            {
                int baseCheck = CanUseDefenceOGCD((uint)WARActionID.原初的血气, true);
                if (baseCheck != 0)
                {
                    return baseCheck;
                }
                double cooldown = SpellHelper.GetSpell((uint)WARActionID.原初的血气).Cooldown.TotalSeconds;
                if (cooldown > Setting.技能提前时间)
                {
                    return -400;
                }

                SpellID = (uint)WARActionID.原初的血气;
                target = Core.Me;
                Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("自动血气");
                if (Setting.自动减伤使用提示)
                {
                    Core.Resolve<MemApiChatMessage>().Toast2($"原初的血气已自动使用", 1, 2000);
                }
                return base.Check();
            }
            else
            {
                int baseCheck = CanUseDefenceOGCD((uint)WARActionID.原初的直觉, true);
                if (baseCheck != 0)
                {
                    return baseCheck;
                }
                double cooldown = SpellHelper.GetSpell((uint)WARActionID.原初的直觉).Cooldown.TotalSeconds;
                if (cooldown > Setting.技能提前时间)
                {
                    return -400;
                }

                SpellID = (uint)WARActionID.原初的直觉;
                target = Core.Me;
                Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("自动血气");
                if (Setting.自动减伤使用提示)
                {
                    Core.Resolve<MemApiChatMessage>().Toast2($"原初的直觉已自动使用", 1, 2000);
                }
                return base.Check();
            }
        }
    }
}

