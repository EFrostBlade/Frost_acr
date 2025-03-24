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

public class 连线大圈 : ITriggerScript
{
    public IBattleChara? 连线1 = null;
    public IBattleChara? 连线2 = null;
    public IBattleChara? 大圈1 = null;
    public IBattleChara? 大圈2 = null;
    public string 连线1名字 = "";
    public string 连线2名字 = "";
    public string 大圈1名字 = "";
    public string 大圈2名字 = "";
    public string 连线1职能 = "";
    public string 连线2职能 = "";
    public string 大圈1职能 = "";
    public string 大圈2职能 = "";
    public bool 浮空 = false;
    public bool Completed = false;

    // 锁对象
    public readonly object _executionLock = new object();

    // 使用整型标记进行原子操作，0：空闲，1：处理中
    public int _isProcessing = 0;

    public bool Check(ScriptEnv scriptEnv, ITriggerCondParams condParams)
    {
        // 目标Icon处理，直接返回（不涉及重复执行的部分）
        if (condParams is TargetIconEffectCondParams { Args0: 126 or 279 } targetIcon)
        {
            if (targetIcon.Args0 == 126)
            {
                if (连线1名字 == "")
                {
                    连线1 = (IBattleChara)targetIcon.Target;
                    连线1名字 = targetIcon.Target.Name.TextValue;
                    Task.Run(() =>
                    {
                        LogHelper.Print("连线1: " + 连线1名字);
                    });
                    return false;
                }
                if (连线2名字 == "")
                {
                    连线2 = (IBattleChara)targetIcon.Target;
                    连线2名字 = targetIcon.Target.Name.TextValue;
                    Task.Run(() =>
                    {
                        LogHelper.Print("连线2: " + 连线2名字);
                    });
                    return false;
                }
            }
            if (targetIcon.Args0 == 279)
            {
                if (大圈1名字 == "")
                {
                    大圈1 = (IBattleChara)targetIcon.Target;
                    大圈1名字 = targetIcon.Target.Name.TextValue;
                    Task.Run(() =>
                    {
                        LogHelper.Print("大圈1: " + 大圈1名字);
                    });
                    return false;
                }
                if (大圈2名字 == "")
                {
                    大圈2 = (IBattleChara)targetIcon.Target;
                    大圈2名字 = targetIcon.Target.Name.TextValue;
                    Task.Run(() =>
                    {
                        LogHelper.Print("大圈2: " + 大圈2名字);
                    });
                    return false;
                }
            }
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
                    if (连线1名字 != "" && 连线2名字 != "" && 大圈1名字 != "" && 大圈2名字 != "" && !浮空)
                    {
                        Share.DebugPointWithText.Clear();
                        连线1职能 = scriptEnv.KV[连线1名字].ToString();
                        连线2职能 = scriptEnv.KV[连线2名字].ToString();
                        大圈1职能 = scriptEnv.KV[大圈1名字].ToString();
                        大圈2职能 = scriptEnv.KV[大圈2名字].ToString();
                        LogHelper.Print("连线1职能: " + 连线1职能);
                        LogHelper.Print("连线2职能: " + 连线2职能);
                        LogHelper.Print("大圈1职能: " + 大圈1职能);
                        LogHelper.Print("大圈2职能: " + 大圈2职能);
                        Share.DebugPointWithText.Add(连线1职能, new Vector3(92, 0, 94));
                        RemoteControlHelper.SetPos(连线1职能, new Vector3(92, 0, 94));
                        Share.DebugPointWithText.Add(连线2职能, new Vector3(92, 0, 94));
                        RemoteControlHelper.SetPos(连线2职能, new Vector3(92, 0, 94));
                        Share.DebugPointWithText.Add(大圈1职能, new Vector3(92, 0, 94));
                        RemoteControlHelper.SetPos(大圈1职能, new Vector3(92, 0, 94));
                        Share.DebugPointWithText.Add(大圈2职能, new Vector3(92, 0, 94));
                        RemoteControlHelper.SetPos(大圈2职能, new Vector3(92, 0, 94));
                        浮空 = true;
                    }
                    if (浮空)
                    {
                        if (连线1.HasAura(3770) && 连线2.HasAura(3770) && 大圈1.HasAura(3770) && 大圈2.HasAura(3770))
                        {
                            LogHelper.Print("连线大圈浮空");
                            Share.DebugPointWithText.Clear();
                            Share.DebugPointWithText.Add(连线1职能, new Vector3(90f, 0, 80.5f));
                            RemoteControlHelper.SetPos(连线1职能, new Vector3(90f, 0, 80.5f));
                            Share.DebugPointWithText.Add(连线2职能, new Vector3(110, 0, 80.5f));
                            RemoteControlHelper.SetPos(连线2职能, new Vector3(110, 0, 80.5f));
                            Share.DebugPointWithText.Add(大圈1职能, new Vector3(82f, 0, 95f));
                            RemoteControlHelper.SetPos(大圈1职能, new Vector3(82f, 0, 95f));
                            Share.DebugPointWithText.Add(大圈2职能, new Vector3(118, 0, 95f));
                            RemoteControlHelper.SetPos(大圈2职能, new Vector3(118, 0, 95f));
                            List<string> allRoles = new List<string> { "MT", "ST", "H1", "H2", "D1", "D2", "D3", "D4" };
                            HashSet<string> usedRoles = new HashSet<string> { 连线1职能, 连线2职能, 大圈1职能, 大圈2职能 };
                            List<string> remainingRoles = allRoles.Where(role => !usedRoles.Contains(role)).ToList();
                            Vector3[] towerCoords = new Vector3[]
                            {
                                new Vector3(92, 0, 89),
                                new Vector3(92, 0, 99),
                                new Vector3(108, 0, 89),
                                new Vector3(108, 0, 99)
                            };

                            for (int i = 0; i < remainingRoles.Count && i < towerCoords.Length; i++)
                            {
                                string role = remainingRoles[i];
                                Vector3 pos = towerCoords[i];
                                Share.DebugPointWithText.Add(role, pos);
                                RemoteControlHelper.SetPos(role, pos);
                            }
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
