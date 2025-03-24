using AEAssist.CombatRoutine.Trigger.Node;
using AEAssist.CombatRoutine.Trigger;
using AEAssist.Helper;
using AEAssist.CombatRoutine.Module;
using System.Numerics;
using AEAssist;
using Dalamud.Game.ClientState.Objects.Types;
using AEAssist.Extension;
using System.Threading;

namespace ScriptTest;

public class 绝对君权 : ITriggerScript
{
    public bool 浮空 = false;
    public bool Completed = false;

    // 锁对象
    public readonly object _executionLock = new object();

    // 使用整型标记进行原子操作，0：空闲，1：处理中
    public int _isProcessing = 0;



    public bool Check(ScriptEnv scriptEnv, ITriggerCondParams condParams)
    {
        // 已经完成，则直接返回
        if (Completed)
            return true;

        // 如果已有任务在处理中，则直接返回当前状态，不重复调度
        if (Interlocked.CompareExchange(ref _isProcessing, 1, 0) != 0)
            return Completed;

        Task.Run(() =>
        {
            try
            {
                lock (_executionLock)
                {
                    Share.DebugPointWithText.Clear();
                    List<Vector3> points = new List<Vector3> { new Vector3(81, 0, 81), new Vector3(119, 0, 81), new Vector3(119, 0, 119), new Vector3(81, 0, 119) };
                    List<IBattleChara> hasBuff = new List<IBattleChara>();
                    List<IBattleChara> noBuff = new List<IBattleChara>();

                    foreach (IBattleChara partyer in PartyHelper.Party)
                    {
                        if (partyer.HasAura(4186))
                        {
                            hasBuff.Add(partyer);
                        }
                        else
                        {
                            noBuff.Add(partyer);
                        }
                    }

                    for (int i = 0; i < hasBuff.Count; i++)
                    {
                        string 职能 = scriptEnv.KV[hasBuff[i].Name.TextValue].ToString();
                        Share.DebugPointWithText.Add(职能, points[i]);
                        RemoteControlHelper.SetPos(职能, points[i]);
                    }

                    foreach (IBattleChara partyer in noBuff)
                    {
                        string 职能 = scriptEnv.KV[partyer.Name.TextValue].ToString();
                        Share.DebugPointWithText.Add(职能, new Vector3(100, 0, 100));
                        RemoteControlHelper.SetPos(职能, new Vector3(100, 0, 100));
                    }
                    Completed = true;
                }
            }
            finally
            {
                // 处理结束后重置标记
                Interlocked.Exchange(ref _isProcessing, 0);
            }
        });

        return Completed;
    }
}
