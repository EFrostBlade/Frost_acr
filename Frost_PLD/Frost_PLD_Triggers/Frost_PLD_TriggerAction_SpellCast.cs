using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Trigger;
using AEAssist.Extension;
using AEAssist.GUI;
using AEAssist.Helper;
using Frost.Frost_PLD.Frost_PLD_Data;
using Frost.Frost_PLD.Frost_PLD_Setting;
using ImGuiNET;
using System;
using System.Threading.Tasks;

namespace Frost.Frost_PLD.Frost_PLD_Triggers
{
    public class Frost_PLD_TriggerAction_SpellCast : ITriggerAction
    {
        public string DisplayName { get; } = "PLD/使用技能";
        public string Remark { get; set; }

        private int _selectIndex;
        private float _waitTime = Frost_PLD_Settings.Instance.cd预检测阈值;
        private bool _forceInsert = false;
        private int _targetIndex = 0;
        private string[] _targetOptions = Enum.GetNames(typeof(Common.TargetType));
        private string _targetInfo = "";

        private enum _SpellArray : uint
        {
            冲刺 = 3u,
            调停 = 16461u,
            钢铁信念 = 28u,
            预警 = 17u,
            极致防御 = 36920u,
            壁垒 = 22u,
            盾阵 = 3542u,
            圣盾阵 = 25746u,
            干预 = 7382u,
            保护 = 27u,
            神圣领域 = 30u,
            圣光幕帘 = 3540u,
            深仁厚泽 = 3541u,
            武装戍卫 = 7385u,
            铁壁 = 7531u,
            挑衅 = 7533u,
            雪仇 = 7535u,
            退避 = 7537u,
            插言 = 7538u,
            下踢 = 7540u,
            亲疏自行 = 7548u,
        }

        private string[] _spellNameArray = Enum.GetNames(typeof(_SpellArray));
        private uint[] _spellValueArray = (uint[])Enum.GetValues(typeof(_SpellArray));

        public bool Draw()
        {
            ImGuiHelper.LeftCombo("技能", ref _selectIndex, _spellNameArray);
            ImGui.Separator();
            // 新增等待时间、强制插入以及目标选择的UI
            ImGui.InputFloat("等待时间（秒）", ref _waitTime, 0.1f, 1f, "%.2f");
            ImGui.Checkbox("强制插入", ref _forceInsert);
            ImGui.Combo("技能目标", ref _targetIndex, _targetOptions, _targetOptions.Length);
            // 如目标类型为 Name 或 DataID, 显示一个输入框来输入目标信息
            string selectedTarget = _targetOptions[_targetIndex];
            if (selectedTarget == "Name")
            {
                ImGui.InputText("目标名称", ref _targetInfo, 100);
            }
            else if (selectedTarget == "DataID")
            {
                ImGui.InputText("目标DataID", ref _targetInfo, 100);
            }
            return true;
        }

        public bool Handle()
        {
            if (_selectIndex >= 0 && _selectIndex < _spellValueArray.Length)
            {
                string skillName = _spellNameArray[_selectIndex];
                // 将触发动作的属性赋值给SC按钮
                Frost_PLD_RotationEntry.scWindow.SetSCDuration(skillName, _waitTime);
                Frost_PLD_RotationEntry.scWindow.SetSCForceInsert(skillName, _forceInsert);
                // 解析目标类型，并将_targetInfo（当有输入时）传入
                Common.TargetType targetValue = (Common.TargetType)Enum.Parse(typeof(Common.TargetType), _targetOptions[_targetIndex]);
                Frost_PLD_RotationEntry.scWindow.SetSCTarget(skillName, targetValue, _targetInfo);
                Frost_PLD_RotationEntry.scWindow.SetSC(skillName, true);
                return true;
            }
            return false;
        }
    }
}
