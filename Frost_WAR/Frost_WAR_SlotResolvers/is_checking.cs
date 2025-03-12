using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Define;
using AEAssist.Extension;
using AEAssist.Helper;
using Dalamud.Game.ClientState.Objects.Types;

namespace Frost.Frost_WAR.Frost_WAR_SlotResolvers
{
    internal class inChecking : Frost_WAR_ISlotResolver
    {
        public override int Check()
        {
            BattleData.isChecking = true;
            if (Core.Me.TargetObject != null)
            {
                BattleData.目标距离 = GameObjectExtension.Distance(Core.Me, Core.Me.GetCurrTarget(), DistanceMode.IgnoreAll);
                BattleData.目标是否在近战范围内 = BattleData.目标距离 <= (float)SettingMgr.GetSetting<GeneralSettings>().AttackRange;
            }
            return -1;
        }
    }
}
