using AEAssist.GUI;
using ImGuiNET;
using Frost.Common;
using Lumina.Excel.Sheets;

namespace Frost.Frost_BLM.Frost_BLM_Setting;

public class Frost_BLM_SettingUI
{
    public static Frost_BLM_SettingUI Instance = new();
    
    public void Draw()
    {
            DrawSettings();
    }

    public static void DrawSettings() 
    {
        ImGui.Text("Frost_BLM acr设置");
        float cd预检测阈值 = (float)Frost_BLM_Settings.Instance.cd预检测阈值;
        if (ImGui.InputFloat("技能使用等待时间", ref cd预检测阈值))
        {
            Frost_BLM_Settings.Instance.cd预检测阈值 = (float)cd预检测阈值;
            Frost_BLM_Settings.Instance.Save();
        }
        if (ImGui.Checkbox("自动减伤", ref Frost_BLM_Settings.Instance.自动减伤))
        {
            Frost_BLM_Settings.Instance.Save();
        }
        if (ImGui.Checkbox("自动疾跑", ref Frost_BLM_Settings.Instance.自动疾跑))
        {
            Frost_BLM_Settings.Instance.Save();
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
        foreach (var skill in Frost_BLM_Settings.Instance.DefaultTargets.Keys.ToList())
        {
            // 第一列：显示技能名称
            ImGui.Text(skill);
            ImGui.NextColumn();

            // 第二列：显示下拉框
            TargetType currentTarget = Frost_BLM_Settings.Instance.DefaultTargets[skill];
            int currentIndex = (int)currentTarget;
            if (ImGui.Combo($"###DefaultTarget_{skill}", ref currentIndex, targetTypes, targetTypes.Length))
            {
                Frost_BLM_Settings.Instance.DefaultTargets[skill] = (TargetType)currentIndex;
                Frost_BLM_Settings.Instance.Save();
            }
            ImGui.NextColumn();

            // 第三列：如果目标类型为 Name 或 DataID，显示输入框，否则显示“-”
            if ((TargetType)currentIndex == TargetType.Name || (TargetType)currentIndex == TargetType.DataID)
            {
                string extra = "";
                if (Frost_BLM_Settings.Instance.DefaultTargetsInfo.ContainsKey(skill))
                    extra = Frost_BLM_Settings.Instance.DefaultTargetsInfo[skill];
                if (ImGui.InputText($"###Extra_{skill}", ref extra, 20))
                {
                    Frost_BLM_Settings.Instance.DefaultTargetsInfo[skill] = extra;
                    Frost_BLM_Settings.Instance.Save();
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
            Frost_BLM_Settings.Instance.Save();
        }
    }
}
