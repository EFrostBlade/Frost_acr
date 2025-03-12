using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Extension;
using AEAssist.Helper;
using AEAssist.MemoryApi;
using Dalamud.Game.ClientState.Objects.Types;
using Frost.Frost_WAR.Frost_WAR_Data;
using Frost.Frost_WAR.Frost_WAR_Setting;

namespace Frost.Frost_WAR.Frost_WAR_SlotResolvers
{
    internal class 前置_自动死斗 : Frost_WAR_ISlotResolver
    {
        public override int Check()
        {
            if (!Setting.自动减伤)
            {
                return -1;
            }
            if (Core.Me.CurrentHpPercent() > (float)Frost_WAR_Settings.Instance.死斗血量阈值 / 100)
            {
                return -2;
            }
            int baseCheck = CanUseDefenceOGCD((uint)WARActionID.死斗,false,true);

            if (baseCheck!=0)
            {
                Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("血量低但是死斗不可用");
                return baseCheck;
            }
            double cooldown = SpellHelper.GetSpell((uint)WARActionID.死斗).Cooldown.TotalSeconds;
            if (cooldown > Setting.cd预检测阈值)
            {
                Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("血量低但是死斗cd");
                return -401;
            }
            if (cooldown > Setting.技能提前时间)
            {
                return -400;
            }

            Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("自动死斗");
            if (Setting.自动减伤使用提示)
            {
                Core.Resolve<MemApiChatMessage>().Toast2($"死斗已自动使用", 1, 2000);
            }
            SpellID = (uint)WARActionID.死斗;
            target = Core.Me;
            return base.Check();
        }
    }
}

