using AEAssist.CombatRoutine.View.JobView;
using AEAssist.Helper;
using AEAssist.IO;
using System.Numerics;
using Frost.Common;

namespace Frost.Frost_BLM.Frost_BLM_Setting;

/// <summary>
/// 配置文件适合放一些一般不会在战斗中随时调整的开关数据
/// 如果一些开关需要在战斗中调整 或者提供给时间轴操作 那就用QT
/// 非开关类型的配置都放配置里 比如诗人绝峰能量配置
/// </summary>
public class Frost_BLM_Settings
{
    public static Frost_BLM_Settings Instance;

    #region 标准模板代码 可以直接复制后改掉类名即可
    private static string path;
    public static void Build(string settingPath)
    {
        path = Path.Combine(settingPath, nameof(Frost_BLM_Settings), "Frost_BLM_settings.json");
        if (!File.Exists(path))
        {
            Instance = new Frost_BLM_Settings();
            Instance.Save();
            return;
        }
        try
        {
            Instance = JsonHelper.FromJson<Frost_BLM_Settings>(File.ReadAllText(path));
        }
        catch (Exception e)
        {
            Instance = new();
            LogHelper.Error(e.ToString());
        }
    }

    public void Save()
    {
        string directoryPath = Path.GetDirectoryName(path) ?? throw new ArgumentNullException(nameof(directoryPath));
        Directory.CreateDirectory(directoryPath);
        File.WriteAllText(path, JsonHelper.ToJson(this));
    }
    #endregion


    public bool 自动减伤 = true;
    public bool 自动疾跑 = true;
    public float cd预检测阈值 = 3;
    public float 技能提前时间 = 0.2f;
    public Dictionary<string, bool> QT配置 = new();
    public Dictionary<string, TargetType> DefaultTargets = new Dictionary<string, TargetType>
    {
        { "调停", TargetType.当前目标 },
        { "下踢", TargetType.当前目标 },
        { "插言", TargetType.当前目标 },
        { "挑衅", TargetType.当前目标 },
        { "退避", TargetType.搭档 },
        { "干预", TargetType.搭档 },
        { "保护", TargetType.血量最低队友 },
        { "深仁厚泽", TargetType.血量最低队友 }
    };
    // 在 Frost_BLM_Settings 类中增加：
    public Dictionary<string, string> DefaultTargetsInfo = new Dictionary<string, string>();


    public JobViewSave JobViewSave = new()
    {
        MainColor = new Vector4((float)0x66 / 0xFF, (float)0xCC / 0xFF, (float)0xFF / 0xff, 0.5f),
        QtWindowBgAlpha = 0.4f,
    };
    public void SaveQt()
    {
        string[] _qtArray = Frost_BLM_RotationEntry.JobViewWindow.GetQtArray();
        QT配置.Clear();
        foreach (var qt in _qtArray)
        {
            QT配置.Add(qt, Frost_BLM_RotationEntry.JobViewWindow.GetQt(qt));
        }
        Save();
    }

    public void LoadQt()
    {
        foreach (var qt in QT配置)
        {
            Frost_BLM_RotationEntry.JobViewWindow.SetQt(qt.Key, qt.Value);
        }
    }

    public void SetDefaultSC(string name)
    {
        var scWindow = Frost_BLM_RotationEntry.scWindow;
        scWindow.SetSCDuration(name, cd预检测阈值);
        scWindow.SetSCForceInsert(name, false);
        if (DefaultTargets.ContainsKey(name))
        {
            if (DefaultTargets[name] == TargetType.Name || DefaultTargets[name] == TargetType.DataID)
            {
                if (DefaultTargetsInfo.ContainsKey(name))
                {
                    scWindow.SetSCTarget(name, DefaultTargets[name], DefaultTargetsInfo[name]);
                }
                else
                {
                    LogHelper.Print($"技能{name}的默认目标信息未设置");
                }
            }
            else
            {
                scWindow.SetSCTarget(name, DefaultTargets[name]);
            }
        }
        else
        {
            scWindow.SetSCTarget(name, TargetType.自身);
        }
    }


}
