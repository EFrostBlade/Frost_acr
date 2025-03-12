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

namespace Frost.Frost_WAR.Frost_WAR_Triggers
{
    public class 移动到目标点 : ITriggerAction
    {

        public string DisplayName { get; } = "通用/移动到目标点";
        public string Remark { get; set; }

        public float x { get; set; }
        public float z { get; set; }


        public bool Draw()
        {
            return false;
        }

        public bool Handle()
        {

            Vector3 point = new Vector3(x, Core.Me.Position.Y, z);
            Core.Resolve<MemApiMove>().MoveToTarget(point);
            Share.TrustDebugPoint.Clear();
            Share.TrustDebugPoint.Add(Core.Me.Position); 
            Share.TrustDebugPoint.Add(point);
            return true;
        }

    }
}
