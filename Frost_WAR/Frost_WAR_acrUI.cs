﻿using System;
using System.Runtime.CompilerServices;
using System.Text;
using ABI.System;
using AEAssist;
using AEAssist.ACT;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.CombatRoutine.Module.Target;
using AEAssist.CombatRoutine.View.JobView;
using AEAssist.Define;
using AEAssist.Extension;
using AEAssist.Helper;
using AEAssist.JobApi;
using AEAssist.MemoryApi;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;
using Frost.Frost_WAR.Frost_WAR_Setting;
using ImGuiNET;
using ECommons.GameFunctions;
using static Frost.Frost_WAR.Frost_WAR_Data.SpellUtils;
using static Frost.Frost_WAR.Frost_WAR_RotationEventHandler;
using System.Numerics;
using Newtonsoft.Json.Linq;
using Frost.Frost_WAR.Frost_WAR_Data;

namespace Frost.Frost_WAR
{
    public class Frost_WAR_AcrUi : IRotationUI
    {
        public static Frost_WAR_AcrUi UI { get; private set; }

        private List<string> logMessages = new List<string>();
        private bool clearLogRequested = false;

        public void AddLog(string message)
        {
            LogHelper.Info(message);
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            logMessages.Add($"[{timestamp}] {message}");
        }
        public void ClearLog()
        {
            logMessages.Clear();
            clearLogRequested = true;
        }



        public void DrawGeneral(JobViewWindow JobViewWindow)
        {
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
            if (ImGui.Checkbox("自动疾跑", ref Frost_WAR_Settings.Instance.自动疾跑))
            {
                Frost_WAR_Settings.Instance.Save();
            }
            if (ImGui.Checkbox("自动拉怪突进", ref Frost_WAR_Settings.Instance.自动拉怪突进))
            {
                Frost_WAR_Settings.Instance.Save();
            }
            if (ImGui.Checkbox("自动拉怪时使用挑衅", ref Frost_WAR_Settings.Instance.自动挑衅))
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
            //if (ImGui.Checkbox("倒计时1s飞斧开怪", ref Frost_WAR_Settings.Instance.飞斧开怪))
            //{
            //    Frost_WAR_Settings.Instance.Save();
            //}
        }
        static Frost_WAR_AcrUi()
        {
            PostFormRequest();
        }

        public void DrawDev(JobViewWindow JobViewWindow)
        {
            if(ImGui.Button("测试"))
            {
                _ = WebSocketServer.Instance.StartAsync("http://127.0.0.1:17938/");
                LogHelper.Print("尝试启动ws服务");

            }
            if (ImGui.Button("发送"))
            {
                _ = WebSocketServer.Instance.SendMessageAsync("Hello, client!");
                LogHelper.Print("尝试发送消息");

            }
            if (ImGui.Button("关闭"))
            {
                // 停止服务
                WebSocketServer.Instance.Stop();
                LogHelper.Print("尝试关闭ws服务");

            }
            var battleData = Frost_WAR_BattleData.Instance;
            var QT = Frost_WAR_RotationEntry.JobViewWindow;
            DefaultInterpolatedStringHandler interpolatedStringHandler;
            interpolatedStringHandler = new DefaultInterpolatedStringHandler(10, 1);
            long minutes = (battleData.战斗开始时长 / 60000) % 60;
            long seconds = (battleData.战斗开始时长 / 1000) % 60;
            interpolatedStringHandler.AppendLiteral($"战斗开始时长：{minutes}分{seconds}秒，{battleData.战斗开始时长}");
            ImGui.Text(interpolatedStringHandler.ToStringAndClear());
            interpolatedStringHandler = new DefaultInterpolatedStringHandler(10, 1);
            interpolatedStringHandler.AppendLiteral("25m内敌人计数：");
            interpolatedStringHandler.AppendFormatted<int>(TargetHelper.GetNearbyEnemyCount(25));
            ImGui.Text(interpolatedStringHandler.ToStringAndClear());
            interpolatedStringHandler = new DefaultInterpolatedStringHandler(10, 1);
            interpolatedStringHandler.AppendLiteral("5m内敌人计数：");
            interpolatedStringHandler.AppendFormatted<int>(TargetHelper.GetNearbyEnemyCount(5));
            ImGui.Text(interpolatedStringHandler.ToStringAndClear());
            interpolatedStringHandler = new DefaultInterpolatedStringHandler(10, 1);
            interpolatedStringHandler.AppendLiteral("以自身为目标敌人计数：");
            interpolatedStringHandler.AppendFormatted<int>(battleData.以自身为目标的敌人数量);
            ImGui.Text(interpolatedStringHandler.ToStringAndClear());
            interpolatedStringHandler = new DefaultInterpolatedStringHandler(10, 1);
            interpolatedStringHandler.AppendLiteral($"战斗状态：{battleData.当前战斗状态}");
            ImGui.Text(interpolatedStringHandler.ToStringAndClear());
            interpolatedStringHandler = new DefaultInterpolatedStringHandler(10, 1);
            interpolatedStringHandler.AppendLiteral($"是否开盾：{battleData.是否开盾}");
            ImGui.Text(interpolatedStringHandler.ToStringAndClear());
            interpolatedStringHandler = new DefaultInterpolatedStringHandler(10, 1);
            interpolatedStringHandler.AppendLiteral($"自身坐标：{Core.Me.Position}");
            ImGui.Text(interpolatedStringHandler.ToStringAndClear());
            interpolatedStringHandler = new DefaultInterpolatedStringHandler(10, 1);
            interpolatedStringHandler.AppendLiteral("目标的目标：");
            string targetText = "";
            if (Core.Me.GetCurrTarget() == null)
            {
                targetText = "自身无目标";
            }
            else if (Core.Me.GetCurrTarget().GetCurrTarget() == null)
            {
                targetText = Core.Me.GetCurrTarget().DataId.ToString() + " " + Core.Me.GetCurrTarget().Name.ToString();
                targetText += ",";
                targetText += "目标无目标";
            }
            else
            {
                targetText = Core.Me.GetCurrTarget().DataId.ToString() + " " + Core.Me.GetCurrTarget().Name.ToString();
                targetText += ",";
                targetText += Core.Me.GetCurrTarget().GetCurrTarget().DataId.ToString() + " " + Core.Me.GetCurrTarget().GetCurrTarget().Name.ToString();
            }
            interpolatedStringHandler.AppendFormatted<string>(targetText);
            ImGui.Text(interpolatedStringHandler.ToStringAndClear());
            interpolatedStringHandler = new DefaultInterpolatedStringHandler(10, 1);
            interpolatedStringHandler.AppendLiteral($"最近每秒承伤：{battleData.自身每秒承伤.GetValueOrDefault(5)}\t\t{battleData.自身每秒承伤.GetValueOrDefault(4)}\t\t{battleData.自身每秒承伤.GetValueOrDefault(3)}\t\t{battleData.自身每秒承伤.GetValueOrDefault(2)}\t\t{battleData.自身每秒承伤.GetValueOrDefault(1)}");
            ImGui.Text(interpolatedStringHandler.ToStringAndClear());
            interpolatedStringHandler = new DefaultInterpolatedStringHandler(10, 1);
            interpolatedStringHandler.AppendLiteral($"目标距离：{battleData.目标距离}");
            ImGui.Text(interpolatedStringHandler.ToStringAndClear());
            interpolatedStringHandler = new DefaultInterpolatedStringHandler(10, 1);
            interpolatedStringHandler.AppendLiteral($"目标是否在近战范围内：{battleData.目标是否在近战范围内}");
            ImGui.Text(interpolatedStringHandler.ToStringAndClear());
            interpolatedStringHandler = new DefaultInterpolatedStringHandler(10, 1);
            if (battleData.nextOGCD != null)
            {
                interpolatedStringHandler.AppendLiteral($"下个oGCD：{battleData.nextOGCD.Name}");
            }
            else
            {
                interpolatedStringHandler.AppendLiteral($"下个oGCD：null");
            }
            ImGui.Text(interpolatedStringHandler.ToStringAndClear());


            interpolatedStringHandler = new DefaultInterpolatedStringHandler(10, 1);
            interpolatedStringHandler.AppendLiteral($"持有单减：{battleData.持有单减}");
            interpolatedStringHandler.AppendLiteral($"目标减：{battleData.持有目标减}");
            interpolatedStringHandler.AppendLiteral($"持有群减：{battleData.持有群减}");
            interpolatedStringHandler.AppendLiteral($"持有单盾：{battleData.持有单盾}");
            interpolatedStringHandler.AppendLiteral($"持有群盾：{battleData.持有群盾}");

            ImGui.Text(interpolatedStringHandler.ToStringAndClear());
            interpolatedStringHandler = new DefaultInterpolatedStringHandler(10, 1);
            interpolatedStringHandler.AppendLiteral($"自身减伤：{battleData.自身减伤比例}% ");
            interpolatedStringHandler.AppendLiteral($"队伍减伤：{battleData.队伍减伤比例}% ");
            interpolatedStringHandler.AppendLiteral($"自身护盾：{battleData.自身护盾} ");
            interpolatedStringHandler.AppendLiteral($"队伍护盾：{battleData.队伍护盾}");
            ImGui.Text(interpolatedStringHandler.ToStringAndClear());
            interpolatedStringHandler = new DefaultInterpolatedStringHandler(10, 1);
            interpolatedStringHandler.AppendLiteral($"{battleData.buff列表}");
            ImGui.Text(interpolatedStringHandler.ToStringAndClear());
            if (ImGui.Button("清空日志"))
            {
                ClearLog();
            }

            // 创建一个可滚动的区域
            ImGui.BeginChild("LogWindow", new System.Numerics.Vector2(0, 300), true);

            if (clearLogRequested)
            {
                clearLogRequested = false;
            }
            else
            {
                foreach (var message in logMessages)
                {
                    ImGui.Text(message);
                }
            }

            // 自动滚动到底部
            if (ImGui.GetScrollY() >= ImGui.GetScrollMaxY())
            {
                ImGui.SetScrollHereY(1.0f);
            }

            ImGui.EndChild();
        }

        private string GetSkillName(uint skillId)
        {
            if (Enum.IsDefined(typeof(WARActionID), skillId))
            {
                return ((WARActionID)skillId).ToString();
            }
            return "未知";
        }

        private string email = string.Empty;
        private string issueDescription = string.Empty;
        public void DrawUpdate(JobViewWindow JobViewWindow)
        {
            ImGui.Text("问题反馈或建议：");


            ImGui.Text("电子邮箱或联系方式");
            ImGui.InputText("##email", ref email, 100);

            ImGui.Text("反馈或建议");
            ImGui.InputTextMultiline("##issueDescription", ref issueDescription, 200, new System.Numerics.Vector2(0, 100));

            if (ImGui.Button("提交"))
            {
                // 调用 PostFeedback 方法
                PostFeedback(email, issueDescription);
            }
            ImGui.Separator();


            ImGui.Text("更新说明：");

            ImGui.BeginChild("UpdateLogWindow", new System.Numerics.Vector2(0, 300), true);

            ImGui.Text("2024.3.10");
            ImGui.Text("修复了一些会导致ae报错Collection was modified; enumeration operation may not execute.的问题");
            ImGui.Text("2024.3.6");
            ImGui.Text("修复了一个会导致副本人数识别错误的问题");
            ImGui.Text("2024.2.14");
            ImGui.Text("新增时间轴行为跟随目标");
            ImGui.Text("2024.2.10");
            ImGui.Text("优化了目标距离和战斗状态的判断");
            ImGui.Text("修复了一些会导致ae报错但是无伤大雅的小问题");
            ImGui.Text("2024.12.26");
            ImGui.Text("优化了更新数据的方法，不再依赖AE的接口，更加稳定");
            ImGui.Text("增加了是否使用qt控制盾姿的设置");
            ImGui.Text("增加了保持qt状态的功能");
            ImGui.Text("2024.12.24");
            ImGui.Text("增加了自动减伤和自动保留尽毁的设置项时间轴控制");
            ImGui.Text("增加了双目标aoeqt，相应的隐藏了立刻死斗的qt");
            ImGui.Text("增加了尽毁和猛攻的使用提示");
            ImGui.Text("修复了未开启自动挑衅时部分情况下仍会判定需要接仇恨的问题");
            ImGui.Text("2024.12.7");
            ImGui.Text("调整了死刑时减伤使用的优先级");
            ImGui.Text("2024.12.6");
            ImGui.Text("修复了开启奶人qt时会勇猛自己的问题");
            ImGui.Text("修复了开启泄资源qt时不会使用动乱的问题");
            ImGui.Text("2024.11.22");
            ImGui.Text("新增了自动更新时间轴的功能");
            ImGui.Text("2024.11.21");
            ImGui.Text("新增了一些时间轴相关检测和控制");
            ImGui.Text("2024.11.20");
            ImGui.Text("修复自动泰然不会释放的问题");
            ImGui.Text("添加了前置蛮荒的qt，相应的，移除了立刻死斗的qt以保持qt栏的大小");
            ImGui.Text("添加了是否自动保留尽毁的设置项，默认关闭，请需要的前往acr设置开启");
            ImGui.Text("添加了自动拉怪是否使用挑衅的设置项");
            ImGui.Text("2024.11.11");
            ImGui.Text("修复主动攻击开启时不会主动攻击的问题");
            ImGui.Text("添加一系列时间轴相关检测和控制");
            ImGui.Text("2024.11.10");
            ImGui.Text("acr整体逻辑完全重做");
            ImGui.Text("增加超爽4人本自动突进");
            ImGui.Text("2024.10.28");
            ImGui.Text("修复了战斗结束后没红斩也泄qt不会重置的问题");
            ImGui.Text("2024.10.26");
            ImGui.Text("修复了战斗中无目标时立刻类qt不会响应的问题");
            ImGui.Text("修复了等级不足时立刻类qt不会自动释放低级技能的问题");
            ImGui.Text("设置中添加了自动疾跑的开关");
            ImGui.Text("2024.10.25");
            ImGui.Text("修改了蛮荒的逻辑，boss战斗时将不会在目标圈外使用");
            ImGui.Text("修复了无法拉仇恨的怪物id上传错误和重复上传的问题");
            ImGui.Text("修改了减伤技能的优先级，现在会更优先使用减伤技能而不是输出技能");
            ImGui.Text("修复了在小怪战斗即将结束时不会飞斧的问题");
            ImGui.Text("修复了团本识别错误导致不会使用疾跑的问题");
            ImGui.Text("2024.10.23");
            ImGui.Text("添加减伤使用和qt的控制命令，推荐使用命令来使用减伤技能");
            ImGui.Text("添加了新QT：禁用飞斧、立刻爆发药、立刻冲刺、立刻挑衅");
            ImGui.Text("修改QT名称：立刻复仇→立刻戮罪");
            ImGui.Text("2024.10.21");
            ImGui.Text("尝试修复了qt显示异常的问题");
            ImGui.Text("修复了木桩歼灭战等特殊副本无法识别的问题");
            ImGui.Text("修复了手动点击qt可能不触发效果的问题");
            ImGui.Text("修复了战斗外关闭盾姿qt后会一直尝试关盾的问题");
            ImGui.Text("增加反馈和建议功能");


            ImGui.EndChild();
        }

        public void PostFeedback(string email, string issueDescription)
        {
            Task.Run(async () =>
            {
                using (HttpClient client = new HttpClient())
                {
                    var formData = new Dictionary<string, string>
                    {
                        { "email", email },
                        { "issueDescription", issueDescription }
                    };

                    var content = new FormUrlEncodedContent(formData);

                    // 发送 POST 请求
                    HttpResponseMessage response = await client.PostAsync("http://111.6.43.254:26732/feed-back/", content);

                    // 确保请求成功
                    response.EnsureSuccessStatusCode();

                    // 读取响应内容
                    string responseBody = await response.Content.ReadAsStringAsync();
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog($"{responseBody}");
                    Core.Resolve<MemApiChatMessage>().Toast2($"{responseBody}", 1, 2000);
                }
            });
        }


        public void OnDrawUI()
        {
        }

        public static implicit operator Frost_WAR_AcrUi(JobViewWindow v)
        {
            throw new NotImplementedException();
        }
    }
}
