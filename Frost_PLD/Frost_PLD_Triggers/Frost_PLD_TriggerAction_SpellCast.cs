using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Trigger;
using AEAssist.Extension;
using AEAssist.GUI;
using AEAssist.Helper;
using Frost.Frost_PLD.Frost_PLD_Data;
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
        private float _waitTime = 0f;
        private bool _forceInsert = false;
        private int _targetIndex = 0;
        private string[] _targetOptions = Enum.GetNames(typeof(Common.TargetType));

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
            return true;
        }

        public bool Handle()
        {
            if (_selectIndex >= 0 && _selectIndex < _spellValueArray.Length)
            {
                string skillName = _spellNameArray[_selectIndex];
                // 修改：不再释放技能
                // 而是配置对应的SC按钮
                Frost_PLD_RotationEntry.scWindow.SetSCDuration(skillName, _waitTime);
                Frost_PLD_RotationEntry.scWindow.SetSCForceInsert(skillName, _forceInsert);
                // 将下拉选中的目标值转换为枚举
                Common.TargetType targetValue = (Common.TargetType)Enum.Parse(typeof(Common.TargetType), _targetOptions[_targetIndex]);
                Frost_PLD_RotationEntry.scWindow.SetSCTarget(skillName, targetValue);
                Frost_PLD_RotationEntry.scWindow.SetSC(skillName, true);
                Frost_PLD_RotationEntry.Frost_PLD_ArcUI.AddLog($"SC按钮激活: {skillName}, 等待时间: {_waitTime}秒, 强制插入: {_forceInsert}, 目标: {targetValue}");
                return true;
            }

            return false;
        }
    }
}
