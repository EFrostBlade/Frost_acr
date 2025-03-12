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
using Frost.Frost_WAR.Frost_WAR_Data;

namespace Frost.Frost_WAR.Frost_WAR_Triggers
{
    public class 拉怪到目标位置 : ITriggerCond
    {

        public string DisplayName { get; } = "通用/拉怪到目标位置";
        public string Remark { get; set; }

        [LabelName("目标位置范围起始X坐标")]
        public float point1x { get; set; }
        [LabelName("目标位置范围起始Z坐标")]
        public float point1z { get; set; }
        [LabelName("目标位置范围终止X坐标")]
        public float point2x { get; set; }
        [LabelName("目标位置范围终止Z坐标")]
        public float point2z { get; set; }


        public bool Draw()
        {
            return false;
        }

        public bool Handle(ITriggerCondParams triggerCondParams)
        {
            if (Core.Me.TargetObject == null)
            {
                return false;
            }

            Vector3 targetPosition = Core.Me.TargetObject.Position;
            bool isWithinX = (targetPosition.X >= Math.Min(point1x, point2x) && targetPosition.X <= Math.Max(point1x, point2x));
            bool isWithinZ = (targetPosition.Z >= Math.Min(point1z, point2z) && targetPosition.Z <= Math.Max(point1z, point2z));

            if (isWithinX && isWithinZ)
            {
                Share.TrustDebugPoint.Clear();
                return true;
            }

            Vector3 myPosition = Core.Me.Position;
            float centerX = (point1x + point2x) / 2;
            float centerZ = (point1z + point2z) / 2;
            Vector3 endPosition = new Vector3(centerX, myPosition.Y, centerZ);

            Vector3 point = new Vector3();
            int gcd = GCDHelper.GetGCDCooldown();
            if (SpellHelper.GetSpell((uint)WARActionID.猛攻).Charges > (float)Frost_WAR_Setting.Frost_WAR_Settings.Instance.保留猛攻层数 + 1f)
            {
            }
            if (gcd > 1000)
            {
                Vector3 direction = Vector3.Normalize(endPosition - targetPosition);
                point = targetPosition + direction * (8 + Core.Me.TargetObject.HitboxRadius);
            }
            else
            {
                Vector3 direction = Vector3.Normalize(endPosition - targetPosition);
                point = targetPosition + direction * (3 + Core.Me.TargetObject.HitboxRadius);
            }
            Core.Resolve<MemApiMove>().MoveToTarget(point);
            Share.TrustDebugPoint.Clear();
            Share.TrustDebugPoint.Add(Core.Me.Position);
            Share.TrustDebugPoint.Add(point);
            return false;
        }

    }
}
