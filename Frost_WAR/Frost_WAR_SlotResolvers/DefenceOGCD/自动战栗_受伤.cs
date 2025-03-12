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
    internal class 自动战栗_受伤 : Frost_WAR_ISlotResolver
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
            if (BattleData.自身每秒承伤.GetValueOrDefault(3) > 0)
            {
                return -303;
            }
            if (BattleData.自身每秒承伤.GetValueOrDefault(2) > 0)
            {
                return -302;
            }
            if (BattleData.自身每秒承伤.GetValueOrDefault(1) > 0)
            {
                return -301;
            }

            if (BattleData.自身减伤比例 >= 20)
            {
                return -5;
            }
            int baseCheck = CanUseDefenceOGCD((uint)WARActionID.战栗);
            if (baseCheck != 0)
            {
                return baseCheck;
            }
            double cooldown = SpellHelper.GetSpell((uint)WARActionID.战栗).Cooldown.TotalSeconds;
            if (cooldown > Setting.技能提前时间)
            {
                return -400;
            }

            SpellID = (uint)WARActionID.战栗;
            target = Core.Me;
            Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("自动战栗");
            if (Setting.自动减伤使用提示)
            {
                Core.Resolve<MemApiChatMessage>().Toast2($"奶妈划水，战栗已自动使用", 1, 2000);
            }
            return base.Check();
        }
    }
}

