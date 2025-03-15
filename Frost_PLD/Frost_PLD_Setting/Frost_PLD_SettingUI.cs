using AEAssist.GUI;
using ImGuiNET;
using Frost.Common;
using Lumina.Excel.Sheets;

namespace Frost.Frost_PLD.Frost_PLD_Setting;

public class Frost_PLD_SettingUI
{
    public static Frost_PLD_SettingUI Instance = new();
    
    public void Draw()
    {
            DrawSettings();
    }

    public static void DrawSettings()
    {
        ImGui.Text("Frost_PLD acr设置");
        if (ImGui.Checkbox("禁用所有位移技能", ref Frost_PLD_Settings.Instance.禁用所有位移技能))
        {
            Frost_PLD_Settings.Instance.Save();
        }
        float cd预检测阈值 = (float)Frost_PLD_Settings.Instance.cd预检测阈值;
        if (ImGui.InputFloat("技能使用等待时间", ref cd预检测阈值))
        {
            Frost_PLD_Settings.Instance.cd预检测阈值 = (float)cd预检测阈值;
            Frost_PLD_Settings.Instance.Save();
        }
        int 远离投盾圣灵阈值 = Frost_PLD_Settings.Instance.远离投盾圣灵阈值;
        if (ImGui.InputInt("远离boss多久后使用投盾圣灵止损(ms)", ref 远离投盾圣灵阈值))
        {
            Frost_PLD_Settings.Instance.远离投盾圣灵阈值 = 远离投盾圣灵阈值;
            Frost_PLD_Settings.Instance.Save();
        }
        int 远离圣灵蓝量阈值 = Frost_PLD_Settings.Instance.远离圣灵蓝量阈值;
        if (ImGui.InputInt("远离使用圣灵时保留蓝量", ref 远离圣灵蓝量阈值))
        {
            Frost_PLD_Settings.Instance.远离投盾圣灵阈值 = 远离圣灵蓝量阈值;
            Frost_PLD_Settings.Instance.Save();
        }
        int 保留调停层数 = Frost_PLD_Settings.Instance.保留调停层数;
        if (ImGui.SliderInt("保留调停层数", ref 保留调停层数, 0, 2, $"{保留调停层数}层"))
        {
            Frost_PLD_Settings.Instance.保留调停层数 = 保留调停层数;
            Frost_PLD_Settings.Instance.Save();
        }
        if (ImGui.Checkbox("启用qt控制盾姿", ref Frost_PLD_Settings.Instance.启用qt控制盾姿))
        {
            Frost_PLD_Settings.Instance.Save();
        }
        if (ImGui.Checkbox("自动减伤", ref Frost_PLD_Settings.Instance.自动减伤))
        {
            Frost_PLD_Settings.Instance.Save();
        }
        if (ImGui.Checkbox("自动减伤使用提示", ref Frost_PLD_Settings.Instance.自动减伤使用提示))
        {
            Frost_PLD_Settings.Instance.Save();
        }
        if (ImGui.Checkbox("自动拉怪突进", ref Frost_PLD_Settings.Instance.自动拉怪突进))
        {
            Frost_PLD_Settings.Instance.Save();
        }
        if (ImGui.Checkbox("自动疾跑", ref Frost_PLD_Settings.Instance.自动疾跑))
        {
            Frost_PLD_Settings.Instance.Save();
        }
        if (ImGui.Checkbox("自动拉怪时使用挑衅", ref Frost_PLD_Settings.Instance.自动挑衅))
        {
            Frost_PLD_Settings.Instance.Save();
        }
        int 无敌血量阈值 = Frost_PLD_Settings.Instance.无敌血量阈值;
        if (ImGui.SliderInt("无敌血量阈值", ref 无敌血量阈值, 0, 100, $"{无敌血量阈值}%"))
        {
            Frost_PLD_Settings.Instance.无敌血量阈值 = 无敌血量阈值;
            Frost_PLD_Settings.Instance.Save();
        }
        // 增加一个分隔符和标题
        ImGui.Separator();
        ImGui.Text("技能默认目标设置");

        // 设置为 3 列：技能名称、默认目标下拉框、额外输入框
        ImGui.Columns(3, "DefaultTargetColumns", true);

        // 显示表头
        ImGui.Text("技能名称"); ImGui.NextColumn();
        ImGui.Text("默认目标"); ImGui.NextColumn();
        ImGui.Text("名称/DataID"); ImGui.NextColumn();
        ImGui.Separator();

        // 获取枚举所有名称
        string[] targetTypes = Enum.GetNames(typeof(TargetType));

        // 遍历默认目标设置（先复制键列表以防修改中错乱）
        foreach (var skill in Frost_PLD_Settings.Instance.DefaultTargets.Keys.ToList())
        {
            // 第一列：显示技能名称
            ImGui.Text(skill);
            ImGui.NextColumn();

            // 第二列：显示下拉框
            TargetType currentTarget = Frost_PLD_Settings.Instance.DefaultTargets[skill];
            int currentIndex = (int)currentTarget;
            if (ImGui.Combo($"###DefaultTarget_{skill}", ref currentIndex, targetTypes, targetTypes.Length))
            {
                Frost_PLD_Settings.Instance.DefaultTargets[skill] = (TargetType)currentIndex;
                Frost_PLD_Settings.Instance.Save();
            }
            ImGui.NextColumn();

            // 第三列：如果目标类型为 Name 或 DataID，显示输入框，否则显示“-”
            if ((TargetType)currentIndex == TargetType.Name || (TargetType)currentIndex == TargetType.DataID)
            {
                string extra = "";
                if (Frost_PLD_Settings.Instance.DefaultTargetsInfo.ContainsKey(skill))
                    extra = Frost_PLD_Settings.Instance.DefaultTargetsInfo[skill];
                if (ImGui.InputText($"###Extra_{skill}", ref extra, 20))
                {
                    Frost_PLD_Settings.Instance.DefaultTargetsInfo[skill] = extra;
                    Frost_PLD_Settings.Instance.Save();
                }
            }
            else
            {
                ImGui.Text("-");
            }
            ImGui.NextColumn();
        }


        if (ImGui.Button("Save"))
        {
            Frost_PLD_Settings.Instance.Save();
        }
    }
}
