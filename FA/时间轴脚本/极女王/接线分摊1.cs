using AEAssist.CombatRoutine.Trigger.Node;
using AEAssist.CombatRoutine.Trigger;
using AEAssist.Helper;
using AEAssist.CombatRoutine.Module;
using System.Numerics;
using AEAssist;
using Dalamud.Game.ClientState.Objects.Types;
using AEAssist.Extension;
using System.Threading;
using ECommons;

namespace ScriptTest;

public class 接线分摊1 : ITriggerScript
{
    public IBattleChara? MT = null;
    public IBattleChara? ST = null;
    public IBattleChara? H1 = null;
    public IBattleChara? H2 = null;
    public IBattleChara? D1 = null;
    public IBattleChara? D2 = null;
    public IBattleChara? D3 = null;
    public IBattleChara? D4 = null;
    public bool startTethers = false;
    public bool stopMove = false;
    public bool Completed = false;
    public long lastTime = 0;
    public long tethersTime = 0;

    // 锁对象
    public readonly object _executionLock = new object();

    // 使用整型标记进行原子操作，0：空闲，1：处理中
    public int _isProcessing = 0;

    public bool Check(ScriptEnv scriptEnv, ITriggerCondParams condParams)
    {

        // 目标Icon处理，直接返回（不涉及重复执行的部分）
        if (condParams is TetherCondParams { Args0: 89 } Tether)
        {
            if (!startTethers)
            {
                tethersTime = AI.Instance.BattleData.CurrBattleTimeInMs;
                startTethers = true;
            }

            return false;
        }
        // 已经完成，则直接返回
        if (Completed) return true;

        // 如果已有任务在处理中，则直接返回当前状态，不重复调度
        if (Interlocked.CompareExchange(ref _isProcessing, 1, 0) != 0)
            return Completed;

        Task.Run(() =>
        {
            try
            {
                lock (_executionLock)
                {
                    if (AI.Instance.BattleData.CurrBattleTimeInMs - lastTime > 100)
                    {
                        lastTime = AI.Instance.BattleData.CurrBattleTimeInMs;
                        if (!stopMove)
                        {
                            foreach (IBattleChara partyer in PartyHelper.Party)
                            {
                                if (partyer == null)
                                    continue;
                                if (partyer.Position.Z >= 104)
                                {
                                    RemoteControlHelper.SetPos(scriptEnv.KV[partyer.Name.TextValue].ToString(), new Vector3(100, 0, 90));
                                }
                            }
                            LogHelper.Print("绿玩移动");
                            RemoteControlHelper.MoveTo("MT", new Vector3(100, 0, 105));
                            RemoteControlHelper.MoveTo("ST", new Vector3(100, 0, 105));
                            RemoteControlHelper.MoveTo("H1", new Vector3(100, 0, 105));
                            RemoteControlHelper.MoveTo("H2", new Vector3(100, 0, 105));
                            RemoteControlHelper.MoveTo("D1", new Vector3(100, 0, 105));
                            RemoteControlHelper.MoveTo("D2", new Vector3(100, 0, 105));
                            RemoteControlHelper.MoveTo("D3", new Vector3(100, 0, 105));
                            RemoteControlHelper.MoveTo("D4", new Vector3(100, 0, 105));
                        }
                        if (startTethers ==true&&AI.Instance.BattleData.CurrBattleTimeInMs - tethersTime > 3000)
                        {
                            LogHelper.Print("停止绿玩移动");
                            stopMove = true;
                            RemoteControlHelper.MoveStop("MT");
                            RemoteControlHelper.MoveStop("ST");
                            RemoteControlHelper.MoveStop("H1");
                            RemoteControlHelper.MoveStop("H2");
                            RemoteControlHelper.MoveStop("D1");
                            RemoteControlHelper.MoveStop("D2");
                            RemoteControlHelper.MoveStop("D3");
                            RemoteControlHelper.MoveStop("D4");
                            Share.DebugPointWithText.Clear();
                            Share.DebugPointWithText.Add("MT", new Vector3(100, 0, 100));
                            RemoteControlHelper.SetPos("MT", new Vector3(100, 0, 100));
                            Share.DebugPointWithText.Add("ST", new Vector3(100, 0, 100));
                            RemoteControlHelper.SetPos("ST", new Vector3(100, 0, 100));
                            Share.DebugPointWithText.Add("H1", new Vector3(100, 0, 100));
                            RemoteControlHelper.SetPos("H1", new Vector3(100, 0, 100));
                            Share.DebugPointWithText.Add("H2", new Vector3(100, 0, 100));
                            RemoteControlHelper.SetPos("H2", new Vector3(100, 0, 100));
                            Share.DebugPointWithText.Add("D1", new Vector3(100, 0, 92.5f));
                            RemoteControlHelper.SetPos("D1", new Vector3(100, 0, 92.5f));
                            Share.DebugPointWithText.Add("D2", new Vector3(100, 0, 92.5f));
                            RemoteControlHelper.SetPos("D2", new Vector3(100, 0, 92.5f));
                            Share.DebugPointWithText.Add("D3", new Vector3(100, 0, 100));
                            RemoteControlHelper.SetPos("D3", new Vector3(100, 0, 100));
                            Share.DebugPointWithText.Add("D4", new Vector3(100, 0, 100));
                            RemoteControlHelper.SetPos("D4", new Vector3(100, 0, 100));
                            Completed = true;
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
