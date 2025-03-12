using AEAssist.CombatRoutine.Trigger;
using AEAssist.GUI;
using ImGuiNET;

namespace Frost.Frost_PLD.Frost_PLD_Triggers
{

    //杩欎釜绫讳篃鍙互瀹屽叏澶嶅埗 鏀逛竴涓嬩笂闈㈢殑namespace鍜屽QT鐨勫紩鐢ㄥ氨琛?
    public class Frost_PLD_TriggerAction_QT : ITriggerAction
    {
        public string DisplayName { get; } = "PLD/QT";
        public string Remark { get; set; }

        public string Key = "";
        public bool Value;

        // 杈呭姪鏁版嵁 鍥犱负鏄痯rivate 鎵€浠ヤ笉瀛樻。
        private int _selectIndex;
        private string[] _qtArray;

        public Frost_PLD_TriggerAction_QT()
        {
            _qtArray = Frost_PLD_RotationEntry.JobViewWindow.GetQtArray();
        }

        public bool Draw()
        {
            _selectIndex = Array.IndexOf(_qtArray, Key);
            if (_selectIndex == -1)
            {
                _selectIndex = 0;
            }
            ImGuiHelper.LeftCombo("QTKey", ref _selectIndex, _qtArray);
            Key = _qtArray[_selectIndex];
            ImGui.SameLine();
            using (new GroupWrapper())
            {
                ImGui.Checkbox("", ref Value);
            }
            return true;
        }

        public bool Handle()
        {
            Frost_PLD_RotationEntry.JobViewWindow.SetQt(Key, Value);
            Frost_PLD_RotationEntry.Frost_PLD_ArcUI.AddLog($"QT变更: {Key} = {Value}");
            return true;
        }
    }
}
