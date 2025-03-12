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
    public class 自动减伤 : ITriggerAction
    {

        public string DisplayName { get; } = "WAR/设置/自动减伤";
        public string Remark { get; set; }

        public bool f { get; set; }


        public bool Draw()
        {
            ImGui.Text("是否启用自动减伤");
            return false;
        }

        public bool Handle()
        {

            Frost_WAR_Settings.Instance.自动减伤 = f;
            return true;
        }

    }
}
