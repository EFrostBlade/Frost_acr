using AEAssist.GUI;
using ImGuiNET;

namespace Frost.Frost_WAR.Frost_WAR_Setting;

public class Frost_WAR_SettingUI
{
    public static Frost_WAR_SettingUI Instance = new();
    public Frost_WAR_Settings Frost_WAR_Settings = Frost_WAR_Settings.Instance;
    
    public void Draw()
    {
        ImGui.Text("Frost_WAR acr设置");
        if (ImGui.Checkbox("禁用所有位移技能", ref Frost_WAR_Settings.Instance.禁用所有位移技能))
        {
            Frost_WAR_Settings.Instance.Save();
        }
        int 续红斩时间秒 = Frost_WAR_Settings.Instance.续红斩时间;
        if (ImGui.InputInt("续红斩时间(秒)", ref 续红斩时间秒))
        {
            Frost_WAR_Settings.Instance.续红斩时间 = 续红斩时间秒;
            Frost_WAR_Settings.Instance.Save();
        }
        float cd预检测阈值 = (float)Frost_WAR_Settings.Instance.cd预检测阈值;
        if (ImGui.InputFloat("立刻类QT在cd即将多少秒内转好不关闭", ref cd预检测阈值))
        {
            Frost_WAR_Settings.Instance.cd预检测阈值 = (double)cd预检测阈值;
            Frost_WAR_Settings.Instance.Save();
        }
        int 保留猛攻层数 = Frost_WAR_Settings.Instance.保留猛攻层数;
        if (ImGui.SliderInt("保留猛攻层数", ref 保留猛攻层数, 0, 3, $"{保留猛攻层数}层"))
        {
            Frost_WAR_Settings.Instance.保留猛攻层数 = 保留猛攻层数;
            Frost_WAR_Settings.Instance.Save();
        }
        if (ImGui.Checkbox("自动保留尽毁", ref Frost_WAR_Settings.Instance.自动保留尽毁))
        {
            Frost_WAR_Settings.Instance.Save();
        }
        ImGui.TextWrapped("开启后将会在合适时候自动保留尽毁并吃爆发药打双尽毁，关闭后将不会自动使用第二次爆发药");
        ImGui.TextWrapped("开启后可能会导致只有1g的蛮荒窗口因为不在boss目标圈内而无法使用，请自行开启允许突进或调整与boss的距离");
        if (ImGui.Checkbox("蛮荒即将过期时自动无位移打出", ref Frost_WAR_Settings.Instance.自动无位移蛮荒))
        {
            Frost_WAR_Settings.Instance.Save();
        }
        if (ImGui.Checkbox("倒数1s使用飞斧开怪", ref Frost_WAR_Settings.Instance.飞斧开怪))
        {
            Frost_WAR_Settings.Instance.Save();
        }
        void AddToUnVisibleList(string item)
        {
            if (!Frost_WAR_Settings.Instance.JobViewSave.QtUnVisibleList.Contains(item))
            {
                Frost_WAR_Settings.Instance.JobViewSave.QtUnVisibleList.Add(item);
            }
        }
        if (ImGui.Checkbox("启用qt控制盾姿", ref Frost_WAR_Settings.Instance.启用qt控制盾姿))
        {
            Frost_WAR_Settings.Instance.Save();
            if (!Frost_WAR_Settings.Instance.启用qt控制盾姿)
            {
                Frost_WAR_Settings.Instance.JobViewSave.QtUnVisibleList.Clear();
                AddToUnVisibleList("立刻血气");
                AddToUnVisibleList("立刻勇猛");
                AddToUnVisibleList("立刻雪仇");
                AddToUnVisibleList("立刻铁壁");
                AddToUnVisibleList("立刻戮罪");
                AddToUnVisibleList("立刻战栗");
                AddToUnVisibleList("立刻泰然");
                AddToUnVisibleList("立刻摆脱");
                AddToUnVisibleList("立刻防击退");
                AddToUnVisibleList("强制突进");
                AddToUnVisibleList("立刻突进");
                AddToUnVisibleList("立刻冲刺");
                AddToUnVisibleList("立刻爆发药");
                AddToUnVisibleList("立刻挑衅");
                AddToUnVisibleList("立刻死斗");
                AddToUnVisibleList("立刻退避");
                AddToUnVisibleList("立刻勇猛远敏");
                AddToUnVisibleList("立刻勇猛法系");
                AddToUnVisibleList("突进无位移");
                AddToUnVisibleList("盾姿");
            }
            else
            {
                Frost_WAR_Settings.Instance.JobViewSave.QtUnVisibleList.Clear();
                AddToUnVisibleList("立刻血气");
                AddToUnVisibleList("立刻勇猛");
                AddToUnVisibleList("立刻雪仇");
                AddToUnVisibleList("立刻铁壁");
                AddToUnVisibleList("立刻戮罪");
                AddToUnVisibleList("立刻战栗");
                AddToUnVisibleList("立刻泰然");
                AddToUnVisibleList("立刻摆脱");
                AddToUnVisibleList("立刻防击退");
                AddToUnVisibleList("强制突进");
                AddToUnVisibleList("立刻突进");
                AddToUnVisibleList("立刻冲刺");
                AddToUnVisibleList("立刻爆发药");
                AddToUnVisibleList("立刻挑衅");
                AddToUnVisibleList("立刻死斗");
                AddToUnVisibleList("立刻退避");
                AddToUnVisibleList("立刻勇猛远敏");
                AddToUnVisibleList("立刻勇猛法系");
                AddToUnVisibleList("突进无位移");
            }
        }
        if (ImGui.Checkbox("自动减伤", ref Frost_WAR_Settings.Instance.自动减伤))
        {
            Frost_WAR_Settings.Instance.Save();
        }
        if (ImGui.Checkbox("自动减伤使用提示", ref Frost_WAR_Settings.Instance.自动减伤使用提示))
        {
            Frost_WAR_Settings.Instance.Save();
        }
        if (ImGui.Checkbox("自动拉怪突进", ref Frost_WAR_Settings.Instance.自动拉怪突进))
        {
            Frost_WAR_Settings.Instance.Save();
        }
        if (ImGui.Checkbox("自动疾跑", ref Frost_WAR_Settings.Instance.自动疾跑))
        {
            Frost_WAR_Settings.Instance.Save();
        }
        if (ImGui.Checkbox("自动拉怪是使用挑衅", ref Frost_WAR_Settings.Instance.自动挑衅))
        {
            Frost_WAR_Settings.Instance.Save();
        }
        int 死斗血量阈值 = Frost_WAR_Settings.Instance.死斗血量阈值;
        if (ImGui.SliderInt("死斗血量阈值", ref 死斗血量阈值, 0, 100, $"{死斗血量阈值}%"))
        {
            Frost_WAR_Settings.Instance.死斗血量阈值 = 死斗血量阈值;
            Frost_WAR_Settings.Instance.Save();
        }
        int 泰然血量阈值 = Frost_WAR_Settings.Instance.泰然血量阈值;
        if (ImGui.SliderInt("泰然血量阈值", ref 泰然血量阈值, 0, 100, $"{泰然血量阈值}%"))
        {
            Frost_WAR_Settings.Instance.泰然血量阈值 = 泰然血量阈值;
            Frost_WAR_Settings.Instance.Save();
        }
        if (ImGui.Button("Save"))
        {
            Frost_WAR_Settings.Instance.Save();
        }
    }
}
