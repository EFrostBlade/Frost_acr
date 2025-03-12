using AEAssist.CombatRoutine.Module;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Trigger;
using AEAssist.GUI;
using AEAssist.Helper;
using ImGuiNET;
using AEAssist.MemoryApi;
using AEAssist;
using System.Numerics;
using AEAssist.Avoid;
using Frost.Frost_WAR.Frost_WAR_Setting;

namespace Frost.Frost_WAR.Frost_WAR_Triggers
{
    public class 停止跟随 : ITriggerAction
    {

        public string DisplayName { get; } = "通用/停止跟随";
        public string Remark { get; set; }



        public bool Draw()
        {
            return false;
        }

        public bool Handle()
        {

            Frost_WAR_RotationEntry.时间轴自动跟随启用情况=false;
            return true;
        }

    }
}
