using AEAssist.CombatRoutine.View.JobView;
using AEAssist.Helper;
using AEAssist.IO;
using System.Numerics;

namespace Frost.Frost_WAR.Frost_WAR_Setting;

/// <summary>
/// 配置文件适合放一些一般不会在战斗中随时调整的开关数据
/// 如果一些开关需要在战斗中调整 或者提供给时间轴操作 那就用QT
/// 非开关类型的配置都放配置里 比如诗人绝峰能量配置
/// </summary>
public class Frost_WAR_Settings
{
    public static Frost_WAR_Settings Instance;

    #region 标准模板代码 可以直接复制后改掉类名即可
    private static string path;
    public static void Build(string settingPath)
    {
        path = Path.Combine(settingPath, nameof(Frost_WAR_Settings), "Frost_WAR_settings.json");
        if (!File.Exists(path))
        {
            Instance = new Frost_WAR_Settings();
            Instance.Save();
            return;
        }
        try
        {
            Instance = JsonHelper.FromJson<Frost_WAR_Settings>(File.ReadAllText(path));
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


    public int 续红斩时间 = 16;
    public bool 飞斧开怪 = true; // 倒计时1s时使用飞斧开怪
    public int 保留猛攻层数 = 1;
    public bool 自动减伤 = true;
    public bool 自动拉怪突进 = true;
    public bool 自动疾跑 = true;
    public bool 自动减伤使用提示 = true;
    public bool 语音提示 = false;
    public int 死斗血量阈值 = 10;
    public int 泰然血量阈值 = 40;
    public double cd预检测阈值 = 3;
    public double 技能提前时间 = 0.2;
    public bool 自动保留尽毁 = false;
    public bool 自动无位移蛮荒 = false;
    public bool 启用qt控制盾姿 = true;
    public bool 自动挑衅 = true;
    public bool 禁用所有位移技能 = false;
    public Dictionary<string, bool> QT配置 = new();

    public JobViewSave JobViewSave = new()
    {
        MainColor = new Vector4((float)0x66 / 0xFF, (float)0xCC / 0xFF, (float)0xFF / 0xff, 0.5f),
        QtWindowBgAlpha = 0.4f,
    };
    public void SaveQt()
    {
        string[] _qtArray = Frost_WAR_RotationEntry.JobViewWindow.GetQtArray();
        QT配置.Clear();
        foreach (var qt in _qtArray)
        {
            QT配置.Add(qt, Frost_WAR_RotationEntry.JobViewWindow.GetQt(qt));
        }
        Save();
    }

    public void LoadQt()
    {
        foreach (var qt in QT配置)
        {
            Frost_WAR_RotationEntry.JobViewWindow.SetQt(qt.Key, qt.Value);
        }
    }


}
