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
    internal class 自动摆脱_aoe : Frost_WAR_ISlotResolver
    {
        public override int Check()
        {
            if (!Setting.自动减伤)
            {
                return -1;
            }
            if (BattleData.当前目标 == null)
            {
                return -2;
            }
            if (!TargetHelper.targetCastingIsBossAOE(BattleData.当前目标, 4000))
            {
                return -3;
            }
            if (BattleData.队伍减伤比例 >= 15)
            {
                return -4;
            }
            if(SpellHelper.GetSpell((uint)WARActionID.雪仇).RecentlyUsed(4000))
            {
                return -5;
            }
            int baseCheck = CanUseDefenceOGCD((uint)WARActionID.摆脱);
            if (baseCheck != 0)
            {
                return baseCheck;
            }
            double cooldown = SpellHelper.GetSpell((uint)WARActionID.摆脱).Cooldown.TotalSeconds;
            if (cooldown > Setting.技能提前时间)
            {
                return -400;
            }

            SpellID = (uint)WARActionID.摆脱;
            target = Core.Me;
            Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("自动摆脱");
            if (Setting.自动减伤使用提示)
            {
                Core.Resolve<MemApiChatMessage>().Toast2($"即将aoe，摆脱已自动使用", 1, 2000);
            }
            return base.Check();
        }
    }
}

