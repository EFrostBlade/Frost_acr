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

            return true;
        }

        public bool Handle()
        {
            if (_selectIndex >= 0 && _selectIndex < _spellValueArray.Length)
            {
                uint spellId = _spellValueArray[_selectIndex];
                string spellName = _spellNameArray[_selectIndex];

                // 使用Task.Run避免阻塞主线程
                Task.Run(async () =>
                {
                    try
                    {
                        // 检查技能是否可用
                        if (CanUseSpell(spellId))
                        {
                            // 对技能类型进行分类处理
                            await CastSpell(spellId);
                            Frost_PLD_RotationEntry.Frost_PLD_ArcUI.AddLog($"成功使用技能: {spellName}");
                        }
                        else
                        {
                            Frost_PLD_RotationEntry.Frost_PLD_ArcUI.AddLog($"技能使用失败: {spellName}，可能在冷却或不符合使用条件");
                        }
                    }
                    catch (Exception ex)
                    {
                        Frost_PLD_RotationEntry.Frost_PLD_ArcUI.AddLog($"技能使用异常: {ex.Message}");
                    }
                });

                return true;
            }

            return false;
        }

        // 检查技能是否可用
        private bool CanUseSpell(uint spellId)
        {
            var spell = SpellHelper.GetSpell(spellId);

            // 技能未解锁
            if (!spell.IsUnlock())
                return false;

            // 技能在冷却中
            if (spell.Cooldown.TotalSeconds > 0)
                return false;

            // 如果是有充能的技能，检查充能数
            if (spell.Charges < 1)
                return false;

            return true;
        }

        // 根据技能类型使用对应的施法方法
        private async Task CastSpell(uint spellId)
        {
            // 尝试发动技能
            var spell = SpellHelper.GetSpell(spellId);

            // 针对特定技能的特殊处理
            switch (spellId)
            {
                case (uint)PLDActionID.干预:
                case (uint)PLDActionID.深仁厚泽:
                case (uint)PLDActionID.保护:
                    // 这些技能需要目标，如果有队友目标则使用，否则选择队伍中血量最低成员
                    await CastSpellOnPartyMember(spellId);
                    break;

                case (uint)PLDActionID.挑衅:
                    // 挑衅技能需要敌人目标
                    await CastSpellOnEnemy(spellId);
                    break;

                default:
                    // 默认使用技能（无目标或以自身为目标）
                    await spell.Cast();
                    break;
            }
        }

        // 对队友使用技能
        private async Task CastSpellOnPartyMember(uint spellId)
        {
        }

        // 对敌人使用技能
        private async Task CastSpellOnEnemy(uint spellId)
        { }
    }
}
