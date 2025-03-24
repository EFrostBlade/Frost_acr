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

public class 放陨石 : ITriggerScript
{
    public IBattleChara? MT = null;
    public IBattleChara? ST = null;
    public IBattleChara? H1 = null;
    public IBattleChara? H2 = null;
    public IBattleChara? D1 = null;
    public IBattleChara? D2 = null;
    public IBattleChara? D3 = null;
    public IBattleChara? D4 = null;
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
                    MT = (IBattleChara)scriptEnv.KV["MT"];
                    ST = (IBattleChara)scriptEnv.KV["ST"];
                    H1 = (IBattleChara)scriptEnv.KV["H1"];
                    H2 = (IBattleChara)scriptEnv.KV["H2"];
                    D1 = (IBattleChara)scriptEnv.KV["D1"];
                    D2 = (IBattleChara)scriptEnv.KV["D2"];
                    D3 = (IBattleChara)scriptEnv.KV["D3"];
                    D4 = (IBattleChara)scriptEnv.KV["D4"];
                    if (MT != null && ST != null && H1 != null && H2 != null && D1 != null && D2 != null && D3 != null && D4 != null)
                    {
                        if (!浮空)
                        {
                            Share.DebugPointWithText.Clear();
                            if (MT.HasAura(3770))
                            {
                                Share.DebugPointWithText.Add("MT", new Vector3(92, 0, 94));
                                RemoteControlHelper.SetPos("MT", new Vector3(92, 0, 94));
                            }
                            if (ST.HasAura(3770))
                            {
                                Share.DebugPointWithText.Add("ST", new Vector3(92, 0, 94));
                                RemoteControlHelper.SetPos("ST", new Vector3(92, 0, 94));
                            }
                            if (!H1.HasAura(3770))
                            {
                                Share.DebugPointWithText.Add("H1", new Vector3(92, 0, 94));
                                RemoteControlHelper.SetPos("H1", new Vector3(92, 0, 94));
                            }
                            if (!H2.HasAura(3770))
                            {
                                Share.DebugPointWithText.Add("H2", new Vector3(92, 0, 94));
                                RemoteControlHelper.SetPos("H2", new Vector3(92, 0, 94));
                            }
                            if (D1.HasAura(3770))
                            {
                                Share.DebugPointWithText.Add("D1", new Vector3(92, 0, 94));
                                RemoteControlHelper.SetPos("D1", new Vector3(92, 0, 94));
                            }
                            if (D2.HasAura(3770))
                            {
                                Share.DebugPointWithText.Add("D2", new Vector3(92, 0, 94));
                                RemoteControlHelper.SetPos("D2", new Vector3(92, 0, 94));
                            }
                            if (!D3.HasAura(3770))
                            {
                                Share.DebugPointWithText.Add("D3", new Vector3(92, 0, 94));
                                RemoteControlHelper.SetPos("D3", new Vector3(92, 0, 94));
                            }
                            if (!D4.HasAura(3770))
                            {
                                Share.DebugPointWithText.Add("D4", new Vector3(92, 0, 94));
                                RemoteControlHelper.SetPos("D4", new Vector3(92, 0, 94));
                            }
                            浮空 = true;
                        }
                        if (浮空)
                        {
                            if (!MT.HasAura(3770) && !ST.HasAura(3770) && H1.HasAura(3770) && H2.HasAura(3770) && !D1.HasAura(3770) && !D2.HasAura(3770) && D3.HasAura(3770) && D4.HasAura(3770))
                            {
                                Share.DebugPointWithText.Clear();
                                Share.DebugPointWithText.Add("MT", new Vector3(92, 0, 86.2f));
                                RemoteControlHelper.SetPos("MT", new Vector3(92, 0, 86.2f));
                                Share.DebugPointWithText.Add("ST", new Vector3(108, 0, 86.2f));
                                RemoteControlHelper.SetPos("ST", new Vector3(108, 0, 86.2f));
                                Share.DebugPointWithText.Add("H1", new Vector3(100, 0, 94));
                                RemoteControlHelper.SetPos("H1", new Vector3(100, 0, 94));
                                Share.DebugPointWithText.Add("H2", new Vector3(100, 0, 94));
                                RemoteControlHelper.SetPos("H2", new Vector3(100, 0, 94));
                                Share.DebugPointWithText.Add("D1", new Vector3(92, 0, 95f));
                                RemoteControlHelper.SetPos("D1", new Vector3(92, 0, 95f));
                                Share.DebugPointWithText.Add("D2", new Vector3(108, 0, 95f));
                                RemoteControlHelper.SetPos("D2", new Vector3(108, 0, 95f));
                                Share.DebugPointWithText.Add("D3", new Vector3(100, 0, 94));
                                RemoteControlHelper.SetPos("D3", new Vector3(100, 0, 94));
                                Share.DebugPointWithText.Add("D4", new Vector3(100, 0, 94));
                                RemoteControlHelper.SetPos("D4", new Vector3(100, 0, 94));
                                Completed = true;
                            }
                        }
                    }
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
