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

public class 获取职能信息 : ITriggerScript
{
    public IBattleChara? MT = null;
    public IBattleChara? ST = null;
    public IBattleChara? H1 = null;
    public IBattleChara? H2 = null;
    public IBattleChara? D1 = null;
    public IBattleChara? D2 = null;
    public IBattleChara? D3 = null;
    public IBattleChara? D4 = null;
    public bool Completed = false;

    // 锁对象
    public readonly object _executionLock = new object();

    // 使用整型标记进行原子操作，0：空闲，1：处理中
    public int _isProcessing = 0;

    public void UpdatePartyRoles(ScriptEnv scriptEnv)
    {
        lock (_executionLock)
        {
            foreach (IBattleChara partyer in PartyHelper.Party)
            {
                if (partyer == null)
                    continue;
                string role = RemoteControlHelper.GetRoleByPlayerName(partyer.Name.TextValue) ?? "";
                if (string.IsNullOrEmpty(role))
                {
                    if (partyer.IsTank())
                        role = (MT == null) ? "MT" : (ST == null ? "ST" : "");
                    else if (partyer.IsHealer())
                        role = (H1 == null) ? "H1" : (H2 == null ? "H2" : "");
                    else if (partyer.IsMelee())
                        role = (D1 == null) ? "D1" : (D2 == null ? "D2" : "");
                    else if (partyer.IsRanged())
                        role = (D3 == null) ? "D3" : "";
                    else if (partyer.IsCaster())
                        role = (D4 == null) ? "D4" : "";
                    else
                    {
                        if (MT == null) role = "MT";
                        else if (ST == null) role = "ST";
                        else if (H1 == null) role = "H1";
                        else if (H2 == null) role = "H2";
                        else if (D1 == null) role = "D1";
                        else if (D2 == null) role = "D2";
                        else if (D3 == null) role = "D3";
                        else if (D4 == null) role = "D4";
                    }
                }

                // 分配角色
                if (!string.IsNullOrEmpty(role))
                {
                    switch (role)
                    {
                        case "MT":
                            MT = partyer;
                            scriptEnv.KV["MT"] = MT;
                            scriptEnv.KV[partyer.Name.TextValue] = "MT";
                            break;
                        case "ST":
                            ST = partyer;
                            scriptEnv.KV["ST"] = ST;
                            scriptEnv.KV[partyer.Name.TextValue] = "ST";
                            break;
                        case "H1":
                            H1 = partyer;
                            scriptEnv.KV["H1"] = H1;
                            scriptEnv.KV[partyer.Name.TextValue] = "H1";
                            break;
                        case "H2":
                            H2 = partyer;
                            scriptEnv.KV["H2"] = H2;
                            scriptEnv.KV[partyer.Name.TextValue] = "H2";
                            break;
                        case "D1":
                            D1 = partyer;
                            scriptEnv.KV["D1"] = D1;
                            scriptEnv.KV[partyer.Name.TextValue] = "D1";
                            break;
                        case "D2":
                            D2 = partyer;
                            scriptEnv.KV["D2"] = D2;
                            scriptEnv.KV[partyer.Name.TextValue] = "D2";
                            break;
                        case "D3":
                            D3 = partyer;
                            scriptEnv.KV["D3"] = D3;
                            scriptEnv.KV[partyer.Name.TextValue] = "D3";
                            break;
                        case "D4":
                            D4 = partyer;
                            scriptEnv.KV["D4"] = D4;
                            scriptEnv.KV[partyer.Name.TextValue] = "D4";
                            break;
                    }
                }
            }
        }
    }

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
                UpdatePartyRoles(scriptEnv);
                LogHelper.Print("职能信息初始化完成");
                LogHelper.Print($"MT: {MT?.Name.TextValue}");
                LogHelper.Print($"ST: {ST?.Name.TextValue}");
                LogHelper.Print($"H1: {H1?.Name.TextValue}");
                LogHelper.Print($"H2: {H2?.Name.TextValue}");
                LogHelper.Print($"D1: {D1?.Name.TextValue}");
                LogHelper.Print($"D2: {D2?.Name.TextValue}");
                LogHelper.Print($"D3: {D3?.Name.TextValue}");
                LogHelper.Print($"D4: {D4?.Name.TextValue}");
                Completed = true;
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
