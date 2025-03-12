

using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.CombatRoutine.Module.AILoop;
using AEAssist.CombatRoutine.Module.Target;
using AEAssist.CombatRoutine.View.JobView;
using AEAssist.Define;
using AEAssist.Extension;
using AEAssist.Helper;
using AEAssist.MemoryApi;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Command;
using Dalamud.Plugin.Services;
using ECommons.DalamudServices;
using Frost.Common;
using Frost.Frost_WAR.Frost_WAR_Data;
using Frost.Frost_WAR.Frost_WAR_Setting;
using Frost.Frost_WAR.Frost_WAR_SlotResolvers;
using Frost.HOOK;
using Lumina.Excel.Sheets;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Security.Cryptography;
using static Frost.Frost_WAR.Frost_WAR_Data.SpellUtils;
namespace Frost.Frost_WAR
{
    internal class Frost_WAR_RotationEventHandler : IRotationEventHandler
    {
        //private Stopwatch _stopwatch;
        //
        //public Frost_WAR_RotationEventHandler()
        //{
        //    _stopwatch = new Stopwatch();
        //    _stopwatch.Start();
        //}

        public async Task OnPreCombat()
        {
            await Frost_WAR_UpdateACRData.OnNoCombat();
        }
        public void OnResetBattle()
        {
            // 重置战斗中缓存的数据
            Frost_WAR_BattleData.Instance = new();

            var QT = Frost_WAR_RotationEntry.JobViewWindow;
            var battleData = Frost_WAR_BattleData.Instance;
            QT.SetQt("保留尽毁", false);
            QT.SetQt("泄资源", false);
            QT.SetQt("没红斩也泄", false);
            QT.SetQt("立刻死斗", false);
            QT.SetQt("立刻退避", false);
            QT.SetQt("立刻血气", false);
            QT.SetQt("立刻勇猛", false);
            QT.SetQt("立刻雪仇", false);
            QT.SetQt("立刻铁壁", false);
            QT.SetQt("立刻戮罪", false);
            QT.SetQt("立刻战栗", false);
            QT.SetQt("立刻泰然", false);
            QT.SetQt("立刻摆脱", false);
            QT.SetQt("强制突进", false);
            QT.SetQt("立刻突进", false);
            QT.SetQt("立刻冲刺", false);
            QT.SetQt("立刻防击退", false);
            QT.SetQt("立刻爆发药", false);
            QT.SetQt("立刻勇猛远敏", false);
            QT.SetQt("立刻勇猛法系", false);
            Frost_WAR_Settings.Instance.SaveQt();

            if (Frost_WAR_DutyData.Instance.无法拉仇恨的敌人列表.Count > 0)
            {
                foreach (var enemy in Frost_WAR_DutyData.Instance.无法拉仇恨的敌人列表)
                {
                    string url = "http://111.6.43.254:26732/cannot-pull-data-better/";
                    var formData = new Dictionary<string, string>
                            {
                                {"Duty",Core.Resolve<MemApiDuty>().DutyInfo.Value.Name.ToString() },
                                { "Name", enemy.Name.ToString() },
                                { "ID", enemy.DataId.ToString() }
                    };

                    // 调用 PostFormRequest 方法
                    PostFormRequest(formData, url);
                }
                Frost_WAR_DutyData.Instance.无法拉仇恨的敌人列表.Clear();
            }


        }
        public static void PostFormRequest(Dictionary<string, string> formData = null, string url = "http://111.6.43.254:26732/null/")
        {
            Task.Run(async () =>
            {
                using (HttpClient client = new HttpClient())
                {
                    if (formData == null)
                    {
                        formData = new Dictionary<string, string>
                        {
                            { "Name", Core.Me.Name.ToString()+" "+Core.Me.HomeWorld.Value.Name.ToString()},
                            { "ID", Share.LocalContentId },
                            {"VIP",Share.VIP.Level.ToString() }
                        };
                    }
                    // 创建一个 FormUrlEncodedContent 对象
                    var content = new FormUrlEncodedContent(formData);

                    // 发送 POST 请求
                    HttpResponseMessage response = await client.PostAsync(url, content);

                    // 确保请求成功
                    response.EnsureSuccessStatusCode();

                    // 读取响应内容
                    string responseBody = await response.Content.ReadAsStringAsync();
                }
            });
        }
        public async Task OnNoTarget()
        {
            await Frost_WAR_UpdateACRData.QtSpell();
        }

        public void OnSpellCastSuccess(Slot slot, Spell spell)
        {
        }

        public void AfterSpell(Slot slot, Spell spell)
        {
            var battleData = Frost_WAR_BattleData.Instance;
            var QT = Frost_WAR_RotationEntry.JobViewWindow;
            if (spell != null)
            {
                if (spell.Id == (uint)WARActionID.死斗)
                {
                    QT.SetQt("立刻死斗", false);
                }
                if (spell.Id == (uint)WARActionID.退避)
                {
                    QT.SetQt("立刻退避", false);
                }
                if (spell.Id == (uint)WARActionID.原初的血气)
                {
                    QT.SetQt("立刻血气", false);
                }
                if (spell.Id == (uint)WARActionID.原初的勇猛)
                {
                    QT.SetQt("立刻勇猛", false);
                    QT.SetQt("立刻勇猛远敏", false);
                    QT.SetQt("立刻勇猛法系", false);
                }
                if (spell.Id == (uint)WARActionID.雪仇)
                {
                    QT.SetQt("立刻雪仇", false);
                }
                if (spell.Id == (uint)WARActionID.铁壁)
                {
                    QT.SetQt("立刻铁壁", false);
                }
                if (spell.Id == (uint)WARActionID.战栗)
                {
                    QT.SetQt("立刻战栗", false);
                }
                if (spell.Id == (uint)WARActionID.泰然自若)
                {
                    QT.SetQt("立刻泰然", false);
                }
                if (spell.Id == (uint)WARActionID.戮罪)
                {
                    QT.SetQt("立刻戮罪", false);
                }
                if (spell.Id == (uint)WARActionID.摆脱)
                {
                    QT.SetQt("立刻摆脱", false);
                }
                if (spell.Id == (uint)WARActionID.亲疏自行)
                {
                    QT.SetQt("立刻防击退", false);
                }
                if (spell.Id == (uint)WARActionID.猛攻)
                {
                    QT.SetQt("强制突进", false);
                    QT.SetQt("立刻突进", false);
                    battleData.猛攻溢出提醒 = false;
                }
                if (spell.Id == (uint)WARActionID.冲刺)
                {
                    QT.SetQt("立刻冲刺", false);
                    Frost_WAR_DutyData.Instance.上次疾跑时间 = DateTime.Now;
                }
                if (spell.SpellCategory == SpellCategory.Potion)
                {
                    QT.SetQt("立刻爆发药", false);
                    QT.SetQt("保留尽毁", false);
                    battleData.已开保留尽毁 = false;
                }
                if (spell.Id == (uint)WARActionID.挑衅)
                {
                    QT.SetQt("立刻挑衅", false);
                }
                if (spell.Id == (uint)WARActionID.原初的解放)
                {
                    battleData.已打解放次数++;
                }
                if (spell.Id == (uint)WARActionID.蛮荒崩裂)
                {
                    battleData.蛮荒过期提醒 = false;
                    if(!QT.GetQt("突进无位移"))
                    {
                        Hook.DisablePatch(PatchType.NoActionMove);
                    }
                }

            }
        }

        public void OnBattleUpdate(int currTimeInMs)
        {
            //if (_stopwatch.ElapsedMilliseconds >= 200)
            //{
            //    _stopwatch.Restart();
            //    Task.Run(() => UpdateBattleData());
            //}
            //Frost_WAR_BattleData.Instance.战斗开始时长 = currTimeInMs;
            Frost_WAR_BattleData.Instance.战斗开始时长 = AI.Instance.BattleData.CurrBattleTimeInMs;
            Frost_WAR_BattleData.Instance.战斗开始分钟数 = (uint)Math.Floor((double)Frost_WAR_BattleData.Instance.战斗开始时长 / 60000) % 60;


        }

        public static IFramework Framework => ECHelper.Framework;
        public async void OnEnterRotation()
        {
            LogHelper.Print("ACR提示", "已切换到Frost的战士acr"); ;
            Frost_WAR_Settings.Instance.LoadQt();
            try
            {
                Svc.Commands.RemoveHandler("/Frost_WAR");
            }
            catch (Exception)
            {
            }
            // 注册命令
            Svc.Commands.AddHandler("/Frost_WAR", new CommandInfo(NewCommandHandler)
            {
                HelpMessage = "Frost的战士acr控制命令"
            });
            LogHelper.Print("ACR提示", "Frost_WAR命令已注册,推荐使用命令来开启减伤技能");// 假设 verifyRequestInstance 是现有的 AEAssist.Verify.VerifyRequest 对象
            Framework.Update += UpdateACR;
            LogHelper.Print("ACR提示", "Framework.Update已注册");
            await Utilities.UpdateTriggerlines("WAR"); 
        }
        public static void UpdateACR(IFramework framework)
        {
            //Do something
            Frost_WAR_UpdateACRData.UpdateBattleData();
            Frost_WAR_UpdateACRData.ModifySlotResolvers(Frost_WAR_RotationEntry.SlotResolvers);
        }
        private static void NewCommandHandler(string command, string args)
        {
            var QT = Frost_WAR_RotationEntry.JobViewWindow;
            switch (args)
            {
                case "立刻血气":
                    QT.SetQt("立刻血气", true);
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("命令变更QT\"立刻血气\"=>true");
                    break;
                case "立刻勇猛":
                    QT.SetQt("立刻勇猛", true);
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("命令变更QT\"立刻勇猛\"=>true");
                    break;
                case "立刻勇猛远敏":
                    QT.SetQt("立刻勇猛远敏", true);
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("命令变更QT\"立刻勇猛远敏\"=>true");
                    break;
                case "立刻勇猛法系":
                    QT.SetQt("立刻勇猛法系", true);
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("命令变更QT\"立刻勇猛法系\"=>true");
                    break;
                case "立刻雪仇":
                    QT.SetQt("立刻雪仇", true);
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("命令变更QT\"立刻雪仇\"=>true");
                    break;
                case "立刻铁壁":
                    QT.SetQt("立刻铁壁", true);
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("命令变更QT\"立刻铁壁\"=>true");
                    break;
                case "立刻戮罪":
                    QT.SetQt("立刻戮罪", true);
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("命令变更QT\"立刻戮罪\"=>true");
                    break;
                case "立刻战栗":
                    QT.SetQt("立刻战栗", true);
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("命令变更QT\"立刻战栗\"=>true");
                    break;
                case "立刻泰然":
                    QT.SetQt("立刻泰然", true);
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("命令变更QT\"立刻泰然\"=>true");
                    break;
                case "立刻摆脱":
                    QT.SetQt("立刻摆脱", true);
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("命令变更QT\"立刻摆脱\"=>true");
                    break;
                case "立刻防击退":
                    QT.SetQt("立刻防击退", true);
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("命令变更QT\"立刻防击退\"=>true");
                    break;
                case "强制突进":
                    QT.SetQt("强制突进", true);
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("命令变更QT\"强制突进\"=>true");
                    break;
                case "立刻突进":
                    QT.SetQt("立刻突进", true);
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("命令变更QT\"立刻突进\"=>true");
                    break;
                case "立刻冲刺":
                    QT.SetQt("立刻冲刺", true);
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("命令变更QT\"立刻冲刺\"=>true");
                    break;
                case "立刻爆发药":
                    QT.SetQt("立刻爆发药", true);
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("命令变更QT\"立刻爆发药\"=>true");
                    break;
                case "立刻挑衅":
                    QT.SetQt("立刻挑衅", true);
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("命令变更QT\"立刻挑衅\"=>true");
                    break;
                case "停手":
                    QT.SetQt("停手", !QT.GetQt("停手"));
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog($"命令变更QT\"停手\"=>{QT.GetQt("停手")}");
                    break;
                case "盾姿":
                    QT.SetQt("盾姿", !QT.GetQt("盾姿"));
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog($"命令变更QT\"盾姿\"=>{QT.GetQt("盾姿")}");
                    break;
                case "1仇":
                    QT.SetQt("1仇", !QT.GetQt("1仇"));
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog($"命令变更QT\"1仇\"=>{QT.GetQt("1仇")}");
                    break;
                case "奶人":
                    QT.SetQt("奶人", !QT.GetQt("奶人"));
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog($"命令变更QT\"奶人\"=>{QT.GetQt("奶人")}");
                    break;
                case "爆发药":
                    QT.SetQt("爆发药", !QT.GetQt("爆发药"));
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog($"命令变更QT\"爆发药\"=>{QT.GetQt("爆发药")}");
                    break;
                case "爆发药不对团辅":
                    QT.SetQt("爆发药不对团辅", !QT.GetQt("爆发药不对团辅"));
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog($"命令变更QT\"爆发药不对团辅\"=>{QT.GetQt("爆发药不对团辅")}");
                    break;
                case "前置尽毁":
                    QT.SetQt("前置尽毁", !QT.GetQt("前置尽毁"));
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog($"命令变更QT\"前置尽毁\"=>{QT.GetQt("前置尽毁")}");
                    break;
                case "保留尽毁":
                    QT.SetQt("保留尽毁", !QT.GetQt("保留尽毁"));
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog($"命令变更QT\"保留尽毁\"=>{QT.GetQt("保留尽毁")}");
                    break;
                case "打完猛攻":
                    QT.SetQt("打完猛攻", !QT.GetQt("打完猛攻"));
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog($"命令变更QT\"打完猛攻\"=>{QT.GetQt("打完猛攻")}");
                    break;
                case "允许突进":
                    QT.SetQt("允许突进", !QT.GetQt("允许突进"));
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog($"命令变更QT\"允许突进\"=>{QT.GetQt("允许突进")}");
                    break;
                case "禁用aoe":
                    QT.SetQt("禁用aoe", !QT.GetQt("禁用aoe"));
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog($"命令变更QT\"禁用aoe\"=>{QT.GetQt("禁用aoe")}");
                    break;
                case "禁用飞斧":
                    QT.SetQt("禁用飞斧", !QT.GetQt("禁用飞斧"));
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog($"命令变更QT\"禁用飞斧\"=>{QT.GetQt("禁用飞斧")}");
                    break;
                case "泄资源":
                    QT.SetQt("泄资源", !QT.GetQt("泄资源"));
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog($"命令变更QT\"泄资源\"=>{QT.GetQt("泄资源")}");
                    break;
                case "没红斩也泄":
                    QT.SetQt("没红斩也泄", !QT.GetQt("没红斩也泄"));
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog($"命令变更QT\"没红斩也泄\"=>{QT.GetQt("没红斩也泄")}");
                    break;
                case "立刻死斗":
                    QT.SetQt("立刻死斗", true);
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("命令变更QT\"立刻死斗\"=>true");
                    break;
                case "立刻退避":
                    QT.SetQt("立刻退避", true);
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("命令变更QT\"立刻退避\"=>true");
                    break;
                default:
                    Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("未知命令");
                    break;
            }

        }

        public void OnExitRotation()
        {
            Frost_WAR_Settings.Instance.SaveQt();
            // 注销命令
            Svc.Commands.RemoveHandler("/Frost_WAR");
            LogHelper.Print("ACR提示", "Frost_WAR命令已注销");
            Framework.Update -= UpdateACR;
            LogHelper.Print("ACR提示", "Framework.Update已注销");
        }

        public async void OnTerritoryChanged()
        {
            await UpdateDutyData(true);
        }

        public async Task UpdateDutyData(bool log = false)
        {
            DutyInfo dutyInfo = new DutyInfo();
            if (dutyInfo.InDuty)
            {
                Frost_PLD_RotationEntry.Frost_PLD_ArcUI.AddLog($"已进入副本{dutyInfo.Id}:{dutyInfo.Name},该副本人数为{dutyInfo.MemberNum}");
            }
        }
    }
}
