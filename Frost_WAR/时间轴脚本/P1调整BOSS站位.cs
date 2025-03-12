using System;
using SystemNumericsVector3 = System.Numerics.Vector3;
using AEAssist;
using AEAssist.CombatRoutine.Trigger;
using AEAssist.CombatRoutine.Trigger.Node;
using AEAssist.Helper;
using System.Reflection;

namespace Congratulation.Script
{
    public class P1调整Boss站位 : ITriggerScript
    {
        public bool Check(ScriptEnv scriptEnv, ITriggerCondParams condParams)
        {
            TP();
            return true;
        }

        public async void TP()
        {
            ///====P1====///
            try
            {
                SystemNumericsVector3 STChangePosition = new SystemNumericsVector3(103.5f, 0.0f, 103.5f);
                SystemNumericsVector3 MTChangePosition = new SystemNumericsVector3(96.5f, 0.0f, 103.5f);
                SystemNumericsVector3 Safeposition = new SystemNumericsVector3(100f, 0.0f, 105f);
                SystemNumericsVector3 Midposition = new SystemNumericsVector3(100f, 0.0f, 100f);
                TeleportToMark("MT", MTChangePosition);
                TeleportToMark("ST", STChangePosition);
                TeleportToMark("D1|D2|D3|D4|H1|H2", Safeposition);
                LogHelper.Print("向下拉怪");
                await Coroutine.Instance.WaitAsync(3500);
                TeleportToMark("MT", Midposition);
                TeleportToMark("ST", Safeposition);
                LogHelper.Print("回到场中");
            }
            catch (Exception ex)
            {
                LogHelper.Print($"调整站位出错: {ex.Message}");
            }
        }

        private async void Post2Splatoon(SystemNumericsVector3 point)
        {
            var ffxivVector3 = ConvertToFFXIVVector3(point);
            var splHelperType = typeof(SplHelper);
            var addPointMethod = splHelperType.GetMethod("AddPoint", BindingFlags.Public | BindingFlags.Static);
            addPointMethod.Invoke(null, new object[] { "测试", ffxivVector3 });
            await Coroutine.Instance.WaitAsync(2000);
        }

        private void TeleportToMark(string Role, SystemNumericsVector3 mark)
        {
            var role = RemoteControlHelper.GetRoleByPlayerName(Role);
            RemoteControlHelper.SetPos(role, mark);
            Post2Splatoon(mark);
            LogHelper.Print($"{Role} TP去{mark}");
        }

        private object ConvertToFFXIVVector3(SystemNumericsVector3 point)
        {
            var ffxivVector3Type = Type.GetType("FFXIVClientStructs.FFXIV.Common.Math.Vector3, FFXIVClientStructs");
            var ffxivVector3 = Activator.CreateInstance(ffxivVector3Type, point.X, point.Y, point.Z);
            return ffxivVector3;
        }
    }
}
