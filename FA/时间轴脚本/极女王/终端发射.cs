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

public class 终端发射 : ITriggerScript
{
    public List<(IBattleChara Chara, uint EntityId, uint Args0)> Tethers = new();
    public IBattleChara? 上1 = null;
    public IBattleChara? 上2 = null;
    public IBattleChara? 右1 = null;
    public IBattleChara? 右2 = null;
    public IBattleChara? 下1 = null;
    public IBattleChara? 下2 = null;
    public IBattleChara? 左1 = null;
    public IBattleChara? 左2 = null;
    public string 上1名字 = "";
    public string 上2名字 = "";
    public string 右1名字 = "";
    public string 右2名字 = "";
    public string 下1名字 = "";
    public string 下2名字 = "";
    public string 左1名字 = "";
    public string 左2名字 = "";
    public string 上1职能 = "";
    public string 上2职能 = "";
    public string 右1职能 = "";
    public string 右2职能 = "";
    public string 下1职能 = "";
    public string 下2职能 = "";
    public string 左1职能 = "";
    public string 左2职能 = "";
    public bool Completed = false;

    // 锁对象
    public readonly object _executionLock = new object();

    // 使用整型标记进行原子操作，0：空闲，1：处理中
    public int _isProcessing = 0;

    public bool Check(ScriptEnv scriptEnv, ITriggerCondParams condParams)
    {
        // 目标Icon处理，直接返回（不涉及重复执行的部分）
        if (condParams is TetherCondParams { Args0: 270 or 271 } Tether)
        {
            Tethers.Add(((IBattleChara)Tether.Left, Tether.Right.EntityId, Tether.Args0));

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
                    if (Tethers.Count == 8)
                    {
                        // 按EntityId排序
                        Tethers.Sort((a, b) => b.EntityId.CompareTo(a.EntityId));

                        // 根据EntityId确定每个方向的玩家
                        var entityIds = Tethers.Select(t => t.EntityId).Distinct().ToList();
                        entityIds.Sort((a, b) => b.CompareTo(a));

                        foreach (var tether in Tethers)
                        {
                            if (tether.EntityId == entityIds[0])
                            {
                                if (tether.Args0 == 270)
                                {
                                    上1 = tether.Chara;
                                    上1名字 = 上1.Name.TextValue;
                                }
                                else
                                {
                                    上2 = tether.Chara;
                                    上2名字 = 上2.Name.TextValue;
                                }
                            }
                            else if (tether.EntityId == entityIds[1])
                            {
                                if (tether.Args0 == 270)
                                {
                                    左1 = tether.Chara;
                                    左1名字 = 左1.Name.TextValue;
                                }
                                else
                                {
                                    左2 = tether.Chara;
                                    左2名字 = 左2.Name.TextValue;
                                }
                            }
                            else if (tether.EntityId == entityIds[2])
                            {
                                if (tether.Args0 == 270)
                                {
                                    右1 = tether.Chara;
                                    右1名字 = 右1.Name.TextValue;
                                }
                                else
                                {
                                    右2 = tether.Chara;
                                    右2名字 = 右2.Name.TextValue;
                                }
                            }
                            else if (tether.EntityId == entityIds[3])
                            {
                                if (tether.Args0 == 270)
                                {
                                    下1 = tether.Chara;
                                    下1名字 = 下1.Name.TextValue;
                                }
                                else
                                {
                                    下2 = tether.Chara;
                                    下2名字 = 下2.Name.TextValue;
                                }
                            }
                        }

                        上1职能 = scriptEnv.KV[上1名字].ToString();
                        LogHelper.Print("上1: " + 上1职能 + " " + 上1名字);
                        上2职能 = scriptEnv.KV[上2名字].ToString();
                        LogHelper.Print("上2: " + 上2职能 + " " + 上2名字);
                        左1职能 = scriptEnv.KV[左1名字].ToString();
                        LogHelper.Print("左1: " + 左1职能 + " " + 左1名字);
                        左2职能 = scriptEnv.KV[左2名字].ToString();
                        LogHelper.Print("左2: " + 左2职能 + " " + 左2名字);
                        右1职能 = scriptEnv.KV[右1名字].ToString();
                        LogHelper.Print("右1: " + 右1职能 + " " + 右1名字);
                        右2职能 = scriptEnv.KV[右2名字].ToString();
                        LogHelper.Print("右2: " + 右2职能 + " " + 右2名字);
                        下1职能 = scriptEnv.KV[下1名字].ToString();
                        LogHelper.Print("下1: " + 下1职能 + " " + 下1名字);
                        下2职能 = scriptEnv.KV[下2名字].ToString();
                        LogHelper.Print("下2: " + 下2职能 + " " + 下2名字);
                    }
                    if (上1 != null && 上2 != null && 右1 != null && 右2 != null && 下1 != null && 下2 != null && 左1 != null && 左2 != null)
                    {
                        Share.DebugPointWithText.Clear();
                        Share.DebugPointWithText.Add(上1职能, new Vector3(100, 0, 81));
                        RemoteControlHelper.SetPos(上1职能, new Vector3(100, 0, 81));
                        Share.DebugPointWithText.Add(上2职能, new Vector3(119, 0, 81));
                        RemoteControlHelper.SetPos(上2职能, new Vector3(119, 0, 81));
                        Share.DebugPointWithText.Add(左1职能, new Vector3(81, 0, 100));
                        RemoteControlHelper.SetPos(左1职能, new Vector3(81, 0, 100));
                        Share.DebugPointWithText.Add(左2职能, new Vector3(81, 0, 81));
                        RemoteControlHelper.SetPos(左2职能, new Vector3(81, 0, 81));
                        Share.DebugPointWithText.Add(右1职能, new Vector3(119, 0, 100));
                        RemoteControlHelper.SetPos(右1职能, new Vector3(119, 0, 100));
                        Share.DebugPointWithText.Add(右2职能, new Vector3(119, 0, 119));
                        RemoteControlHelper.SetPos(右2职能, new Vector3(119, 0, 119));
                        Share.DebugPointWithText.Add(下1职能, new Vector3(100, 0, 119));
                        RemoteControlHelper.SetPos(下1职能, new Vector3(100, 0, 119));
                        Share.DebugPointWithText.Add(下2职能, new Vector3(81, 0, 119));
                        RemoteControlHelper.SetPos(下2职能, new Vector3(81, 0, 119));
                        Completed = true;
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
