using System;
using System.Collections.Generic;
using System.Numerics;
using ImGuiNET; // 使用ImGui进行UI绘制
using AEAssist.Helper;
using Dalamud.Game.ClientState.Objects.Types;
using System.Runtime.CompilerServices;
using AEAssist;
using AEAssist.Extension;

namespace Frost.Common
{
    /// <summary>
    /// 自定义的技能按钮窗口，为按钮添加持续时间和强制插入属性
    /// </summary>
    public class SCWindow
    {
        // 窗口标题
        private string title;

        // 窗口配置
        private bool isVisible = true;
        private Vector2 windowPos = new Vector2(100, 100);
        private Vector2 windowSize = new Vector2(300, 200);
        private float windowAlpha = 0.4f; // 半透明度

        // 按钮集合
        private Dictionary<string, SC> buttons = new Dictionary<string, SC>();

        // 颜色配置
        private readonly Vector4 normalColor = new Vector4(0.2f, 0.6f, 1.0f, 0.4f);      // 蓝色 (非强制插入)
        private readonly Vector4 forceInsertColor = new Vector4(1.0f, 0.2f, 0.2f, 0.4f); // 红色 (强制插入)
        private readonly Vector4 inactiveColor = new Vector4(0.5f, 0.5f, 0.5f, 0.4f);    // 灰色 (未激活)

        // 配置
        private bool showCountdown = true;
        private int buttonWidth = 80;
        private int buttonHeight = 40;
        private int buttonsPerRow = 4;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="title">窗口标题</param>
        public SCWindow()
        {
            this.title = "技能使用";
            // 删除后台倒计时更新任务，改为在 Draw 中更新
            // StartCountdownUpdateTask();
        }

        /// <summary>
        /// 添加按钮
        /// </summary>
        public void AddSC(string name, float duration = 0, bool forceInsert = false, TargetType target = TargetType.自身)
        {
            if (!buttons.ContainsKey(name))
            {
                var button = new SC
                {
                    Name = name,
                    IsActive = false,
                    Duration = duration,
                    ForceInsert = forceInsert,
                    ActivationTime = DateTime.MinValue,
                    Target = target
                };

                buttons[name] = button;
            }
        }

        /// <summary>
        /// 添加带工具提示的按钮
        /// </summary>
        public void AddSC(string name, string tooltip, float duration = 0, bool forceInsert = false, TargetType target = TargetType.自身)
        {
            if (!buttons.ContainsKey(name))
            {
                var button = new SC
                {
                    Name = name,
                    IsActive = false,
                    Duration = duration,
                    ForceInsert = forceInsert,
                    Tooltip = tooltip,
                    ActivationTime = DateTime.MinValue,
                    Target = target
                };

                buttons[name] = button;
            }
        }

        /// <summary>
        /// 添加带点击事件的按钮
        /// </summary>
        public void AddSC(string name, Action<bool> onClick, float duration = 0, bool forceInsert = false, TargetType target = TargetType.自身)
        {
            if (!buttons.ContainsKey(name))
            {
                var button = new SC
                {
                    Name = name,
                    IsActive = false,
                    Duration = duration,
                    ForceInsert = forceInsert,
                    OnClick = onClick,
                    ActivationTime = DateTime.MinValue,
                    Target = target
                };

                buttons[name] = button;
            }
        }

        /// <summary>
        /// 获取按钮状态
        /// </summary>
        public bool GetSC(string name)
        {
            return buttons.TryGetValue(name, out var button) && button.IsActive;
        }
        /// <summary>
        /// 设置按钮状态
        /// </summary>
        public bool SetSC(string name, bool state)
        {
            if (buttons.TryGetValue(name, out var button))
            {
                bool oldState = button.IsActive;

                // 记录设置前的状态，便于调试
                //LogHelper.Print($"按钮 {name} 设置状态：当前={oldState}，目标={state}");

                // 设置新状态
                button.IsActive = state;

                // 如果状态发生了变化
                if (oldState != state)
                {
                    if (state)
                    {
                        // 如果是激活按钮，设置激活时间
                        button.ActivationTime = DateTime.Now;
                    }
                    else
                    {
                        // 如果是取消激活，清理相关状态
                        //LogHelper.Print($"按钮 {name} 的倒计时已停止");
                        button.ActivationTime = DateTime.MinValue;  // 清除激活时间
                    }

                    // 触发按钮事件
                    button.OnClick?.Invoke(state);

                    // 记录日志
                    if (state)
                    {
                        string message = $"{name} 将在{button.Duration}秒内";
                        message = button.ForceInsert ? message + "强制插入" : message + "插入";
                        if (button.Target != null)
                        {
                            message += $",目标为{button.Target}";
                        }
                        LogHelper.Print("技能插入", message);
                    }
                }

                return true;
            }

            return false;
        }
        /// <summary>
        /// 立即停止按钮的倒计时并取消激活
        /// </summary>
        /// <param name="name">按钮名称</param>
        /// <returns>操作是否成功</returns>
        public bool StopSC(string name)
        {
            if (buttons.TryGetValue(name, out var button))
            {
                if (button.IsActive)
                {
                    // 取消激活状态
                    button.IsActive = false;

                    // 清除激活时间
                    button.ActivationTime = DateTime.MinValue;

                    // 触发按钮事件，传递false表示取消激活
                    button.OnClick?.Invoke(false);

                    //LogHelper.Print($"按钮 {name} 的倒计时已手动停止");

                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// 停止所有按钮的倒计时并取消激活
        /// </summary>
        public void StopAllSC()
        {
            List<string> activeButtons = new List<string>();

            // 先收集所有激活的按钮
            foreach (var kvp in buttons)
            {
                if (kvp.Value.IsActive)
                {
                    activeButtons.Add(kvp.Key);
                }
            }

            // 然后停止所有这些按钮的倒计时
            foreach (string name in activeButtons)
            {
                StopSC(name);
            }

            //LogHelper.Print("所有按钮倒计时已停止");
        }
        /// <summary>
        /// 获取按钮持续时间
        /// </summary>
        public float GetSCDuration(string name)
        {
            return buttons.TryGetValue(name, out var button) ? button.Duration : 0;
        }

        /// <summary>
        /// 设置按钮持续时间
        /// </summary>
        public void SetSCDuration(string name, float duration)
        {
            if (buttons.TryGetValue(name, out var button))
            {
                button.Duration = duration;
            }
        }

        ///<summary>
        ///获取按钮强制插入属性
        /// </summary>
        public bool GetSCForceInsert(string name)
        {
            return buttons.TryGetValue(name, out var button) && button.ForceInsert;
        }

        /// <summary>
        /// 设置按钮强制插入属性
        /// </summary>
        public void SetSCForceInsert(string name, bool forceInsert)
        {
            if (buttons.TryGetValue(name, out var button))
            {
                button.ForceInsert = forceInsert;
            }
        }
        ///<summary>
        ///获取目标
        /// </summary>
        public IBattleChara? GetSCTarget(string name)
        {
            TargetType targetType = buttons.TryGetValue(name, out var button) ? button.Target : TargetType.自身;
            switch (targetType)
            {
                case TargetType.自身:
                    return Core.Me;
                case TargetType.当前目标:
                    return Core.Me.GetCurrTarget();
                case TargetType.目标的目标:
                    return Core.Me.GetCurrTarget()?.GetCurrTarget();
                case TargetType.血量最低队友:
                    return TargetGetter.GetLowestHealthPartyMember();
                case TargetType.坦克:
                    return TargetGetter.GetTank();
                case TargetType.治疗:
                    return TargetGetter.GetHealer();
                case TargetType.近战:
                    return TargetGetter.GetMelee();
                case TargetType.远敏:
                    return TargetGetter.GetRanged();
                case TargetType.法系:
                    return TargetGetter.GetCaster();
                case TargetType.搭档:
                    return PartyHelper.GetAnotherTank(Core.Me);
                case TargetType.小队列表1:
                    return TargetGetter.GetPartyMember(1);
                case TargetType.小队列表2:
                    return TargetGetter.GetPartyMember(2);
                case TargetType.小队列表3:
                    return TargetGetter.GetPartyMember(3);
                case TargetType.小队列表4:
                    return TargetGetter.GetPartyMember(4);
                case TargetType.小队列表5:
                    return TargetGetter.GetPartyMember(5);
                case TargetType.小队列表6:
                    return TargetGetter.GetPartyMember(6);
                case TargetType.小队列表7:
                    return TargetGetter.GetPartyMember(7);
                case TargetType.Name:
                    return TargetGetter.GetBattleCharaByName(button.TargetInfo);
                case TargetType.DataID:
                    return TargetGetter.GetBattleCharaByDataID((uint)Convert.ToInt32(button.TargetInfo));
                default:
                    return null;
            }
        }

        ///<summary>
        ///设置目标
        /// </summary>
        public void SetSCTarget(string name, TargetType target, string? targetInfo = null)
        {
            if (buttons.TryGetValue(name, out var button))
            {
                button.Target = target;
                button.TargetInfo = targetInfo;
            }
        }

        /// <summary>
        /// 获取按钮剩余时间
        /// </summary>
        public float GetSCRemainingTime(string name)
        {
            if (buttons.TryGetValue(name, out var button) && button.IsActive && button.Duration > 0)
            {
                TimeSpan elapsed = DateTime.Now - button.ActivationTime;
                return Math.Max(0, button.Duration - (float)elapsed.TotalSeconds);
            }

            return 0;
        }

        /// <summary>
        /// 绘制窗口
        /// </summary>
        public void Draw()
        {
            if (!isVisible) return;

            // 设置窗口属性
            ImGui.SetNextWindowPos(windowPos, ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSize(windowSize, ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowBgAlpha(windowAlpha);

            // 添加 NoTitleBar 标志来隐藏标题栏
            ImGuiWindowFlags flags = ImGuiWindowFlags.NoCollapse |
                                     ImGuiWindowFlags.NoSavedSettings |
                                     ImGuiWindowFlags.AlwaysAutoResize |
                                     ImGuiWindowFlags.NoTitleBar;

            // 绘制窗口
            ImGui.Begin(title, ref isVisible, flags);

            int columnCount = 0;

            // 添加窗口内边距
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(8, 0)); // 增加项目间距

            // 绘制按钮
            foreach (var kvp in buttons)
            {
                string name = kvp.Key;
                SC button = kvp.Value;

                // 在同一行显示多个按钮
                if (columnCount > 0 && columnCount < buttonsPerRow)
                {
                    ImGui.SameLine();
                }

                // 设置按钮颜色
                if (button.IsActive)
                {
                    Vector4 buttonColor = button.ForceInsert ? forceInsertColor : normalColor;
                    ImGui.PushStyleColor(ImGuiCol.Button, buttonColor);
                    ImGui.PushStyleColor(ImGuiCol.ButtonHovered, buttonColor * 1.1f);
                    ImGui.PushStyleColor(ImGuiCol.ButtonActive, buttonColor * 0.9f);
                }
                else
                {
                    ImGui.PushStyleColor(ImGuiCol.Button, inactiveColor);
                    ImGui.PushStyleColor(ImGuiCol.ButtonHovered, inactiveColor * 1.1f);
                    ImGui.PushStyleColor(ImGuiCol.ButtonActive, inactiveColor * 0.9f);
                }

                if (ImGui.Button(name, new Vector2(buttonWidth, buttonHeight)))
                {
                    bool newState = !button.IsActive;
                    //LogHelper.Print($"按钮 {name} 被点击，当前状态={button.IsActive}，将设置为={newState}");
                    SetSC(name, newState);
                }

                // 如果按钮激活且需要显示倒计时，在按钮上叠加倒计时文本
                if (button.IsActive && showCountdown && button.Duration > 0)
                {
                    float remainingTime = GetSCRemainingTime(name);
                    if (remainingTime > 0)
                    {
                        // 获取当前按钮的绘制区域
                        Vector2 rectMin = ImGui.GetItemRectMin();
                        Vector2 rectMax = ImGui.GetItemRectMax();
                        Vector2 center = new Vector2((rectMin.X + rectMax.X) / 2, (rectMin.Y + rectMax.Y) / 2);

                        // 计算倒计时文本的尺寸
                        string countdownText = $"{remainingTime:F2}s";
                        Vector2 textSize = ImGui.CalcTextSize(countdownText);
                        Vector2 textPos = new Vector2(center.X - textSize.X / 2, center.Y + textSize.Y / 2);

                        // 绘制倒计时文本，颜色可自行调整
                        var drawList = ImGui.GetWindowDrawList();
                        uint overlayColor = ImGui.ColorConvertFloat4ToU32(new Vector4(1, 1, 1, 1)); // 白色
                        drawList.AddText(textPos, overlayColor, countdownText);
                    }
                    else
                    {
                        SetSC(name, false);
                        LogHelper.Print("技能插入", $"{name} 在设定时间内使用失败，已自动关闭");
                    }
                }

                // 显示工具提示
                if (!string.IsNullOrEmpty(button.Tooltip) && ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip(button.Tooltip);
                }

                // 更新列计数
                columnCount = (columnCount + 1) % buttonsPerRow;
                if (columnCount == 0)
                {
                    ImGui.NewLine();
                }
            }

            ImGui.PopStyleVar(); // 恢复项目间距
            ImGui.End();

        }


        /// <summary>
        /// 设置窗口位置
        /// </summary>
        public void SetPosition(Vector2 position)
        {
            windowPos = position;
        }

        /// <summary>
        /// 设置窗口大小
        /// </summary>
        public void SetSize(Vector2 size)
        {
            windowSize = size;
        }

        /// <summary>
        /// 设置窗口透明度
        /// </summary>
        public void SetAlpha(float alpha)
        {
            windowAlpha = Math.Clamp(alpha, 0.1f, 1.0f);
        }

        /// <summary>
        /// 设置窗口可见性
        /// </summary>
        public void SetVisible(bool visible)
        {
            isVisible = visible;
        }
    }

    /// <summary>
    /// 技能按钮类
    /// </summary>
    public class SC
    {
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public float Duration { get; set; }
        public bool ForceInsert { get; set; }
        public string? Tooltip { get; set; }
        public Action<bool>? OnClick { get; set; }
        public DateTime ActivationTime { get; set; }
        public TargetType Target { get; set; }
        public string? TargetInfo { get; set; }
    }
    public enum TargetType
    {
        自身,
        当前目标,
        目标的目标,
        血量最低队友,
        坦克,
        治疗,
        近战,
        远敏,
        法系,
        搭档,
        小队列表1,
        小队列表2,
        小队列表3,
        小队列表4,
        小队列表5,
        小队列表6,
        小队列表7,
        Name,
        DataID,
    }
}
