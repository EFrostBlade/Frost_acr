using AEAssist.CombatRoutine.Module.Target;
using AEAssist.CombatRoutine.Trigger.Node;
using AEAssist.CombatRoutine.Trigger;
using AEAssist.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace Frost.Frost_WAR.时间轴脚本.m3s
{
    internal class 距离衰减突进 : ITriggerScript
    {
        public bool Check(ScriptEnv scriptEnv, ITriggerCondParams condParams)
        {
            var targetList = TargetMgr.Instance.Enemys
                .Where(r => r.Value.DataId == 17094)
                .ToList();

            var arenaCenter = new Vector3(100, 0, 100);
            if (condParams is ReceviceAbilityEffectCondParams ae)
            {
                if (ae.ActionId == 37868 || ae.ActionId == 37877)
                {
                    return true;
                }
            }

            if (targetList.Any())
            {
                var target = targetList.First();
                var targetPosition = target.Value.Position;
                var distance = CalculateDistance(targetPosition.X, targetPosition.Z, arenaCenter.X, arenaCenter.Z);
                if (distance < 14.5f) return false;
                return true;
            }
            return false;
        }

        private float CalculateDistance(float x1, float z1, float x2, float z2)
        {
            return (float)Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(z2 - z1, 2));
        }
    }
}
