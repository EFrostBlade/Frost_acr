

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
using Frost.Frost_PLD.Frost_PLD_Data;
using Frost.Frost_PLD.Frost_PLD_Setting;
using Frost.Frost_PLD.Frost_PLD_SlotResolvers;
using Frost.HOOK;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Security.Cryptography;
namespace Frost.Frost_PLD
{
    internal class Frost_PLD_RotationEventHandler : IRotationEventHandler
    {
        //private Stopwatch _stopwatch;
        //
        //public Frost_PLD_RotationEventHandler()
        //{
        //    _stopwatch = new Stopwatch();
        //    _stopwatch.Start();
        //}

        public async Task OnPreCombat()
        {
            await Frost_PLD_UpdateACRData.OnNoCombat();
        }
        public void OnResetBattle()
        {
            // 重置战斗中缓存的数据
            Frost_PLD_BattleData.Instance = new();

            var QT = Frost_PLD_RotationEntry.JobViewWindow;
            var battleData = Frost_PLD_BattleData.Instance;
            QT.SetQt("泄资源", false);
            Frost_PLD_Settings.Instance.SaveQt();

            if (Frost_PLD_DutyData.Instance.无法拉仇恨的敌人列表.Count > 0)
            {
                foreach (var enemy in Frost_PLD_DutyData.Instance.无法拉仇恨的敌人列表)
                {
                    string url = "http://111.6.43.254:26732/cannot-pull-data-better/";
                    var formData = new Dictionary<string, string>
                            {
                                {"Duty",Core.Resolve<MemApiDuty>().DutyInfo.Value.Name.ToString()},
                                { "Name", enemy.Name.ToString() },
                                { "ID", enemy.DataId.ToString() }
                    };

                    // 调用 PostFormRequest 方法
                    PostFormRequest(formData, url);
                }
                Frost_PLD_DutyData.Instance.无法拉仇恨的敌人列表.Clear();
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
            await Frost_PLD_UpdateACRData.SpellCast();
        }

        public void OnSpellCastSuccess(Slot slot, Spell spell)
        {
        }

        public void AfterSpell(Slot slot, Spell spell)
        {
            //LogHelper.Print($"{spell.Id}:{spell.Name}");
            var battleData = Frost_PLD_BattleData.Instance;
            var QT = Frost_PLD_RotationEntry.JobViewWindow;
            var SC = Frost_PLD_RotationEntry.scWindow;
            var settings = Frost_PLD_Settings.Instance;
            if (spell != null)
            {
                if (spell.Id == (uint)PLDActionID.悔罪
                    || spell.Id == (uint)PLDActionID.信念之剑
                    || spell.Id == (uint)PLDActionID.真理之剑
                    || spell.Id == (uint)PLDActionID.英勇之剑)
                {
                    battleData.上次魔法连击技能 = spell.Id;
                    battleData.上次魔法连击时间 = battleData.战斗开始时长;
                }
                else if (spell.Id == (uint)PLDActionID.先锋剑
                    || spell.Id == (uint)PLDActionID.暴乱剑
                    || spell.Id == (uint)PLDActionID.战女神之怒
                    || spell.Id == (uint)PLDActionID.王权剑)
                {
                    battleData.上次魔法连击技能 = 0;
                    battleData.上次魔法连击时间 = 0;
                }

                else if (spell.Id == (uint)PLDActionID.冲刺)
                {
                    string _spellName = spell.Name;
                    if (SC.GetSC(_spellName))
                    {
                        LogHelper.Print("技能使用", $"{_spellName} 已成功对{spell.GetTarget().Name}使用,距离激活{(DateTime.Now - SC.GetSCActivationTime(_spellName)).TotalSeconds:F2}秒");
                        SC.SetSC(_spellName, false);
                        settings.SetDefaultSC(_spellName);
                    }
                    else
                    {
                        LogHelper.Print("技能使用", $"{_spellName} 已成功对{spell.GetTarget().Name}使用，使用了不推荐的方式，推荐使用acr中的技能使用来使用技能");
                    }
                    Frost_PLD_DutyData.Instance.上次疾跑时间 = DateTime.Now;
                }

                else if (spell.Id == (uint)PLDActionID.钢铁信念)
                {
                    string _spellName = spell.Name;
                    if (SC.GetSC(_spellName))
                    {
                        LogHelper.Print("技能使用", $"{_spellName} 已成功对{spell.GetTarget().Name}使用,距离激活{(DateTime.Now - SC.GetSCActivationTime(_spellName)).TotalSeconds:F2}秒");
                        SC.SetSC(_spellName, false);
                        settings.SetDefaultSC(_spellName);
                    }
                }
                else if (spell.Id == (uint)PLDActionID.解除钢铁信念)
                {
                    string _spellName = spell.Name;
                    if (SC.GetSC("钢铁信念"))
                    {
                        LogHelper.Print("技能使用", $"{_spellName} 已成功对{spell.GetTarget().Name}使用,距离激活{(DateTime.Now - SC.GetSCActivationTime(_spellName)).TotalSeconds:F2}秒");
                        SC.SetSC("钢铁信念", false);
                        settings.SetDefaultSC("钢铁信念");
                    }
                    else
                    {
                        LogHelper.Print("技能使用", $"{_spellName} 已成功对{spell.GetTarget().Name}使用，使用了不推荐的方式，推荐使用acr中的技能使用来使用技能");
                    }
                }
                else if (spell.Id == (uint)PLDActionID.极致防御)
                {
                    string _spellName = spell.Name;
                    if (SC.GetSC(_spellName))
                    {
                        LogHelper.Print("技能使用", $"{_spellName} 已成功对{spell.GetTarget().Name}使用,距离激活{(DateTime.Now - SC.GetSCActivationTime(_spellName)).TotalSeconds:F2}秒");
                        SC.SetSC(_spellName, false);
                        settings.SetDefaultSC(_spellName);
                    }
                    else
                    {
                        LogHelper.Print("技能使用", $"{_spellName} 已成功对{spell.GetTarget().Name}使用，使用了不推荐的方式，推荐使用acr中的技能使用来使用技能");
                    }
                }
                else if (spell.Id == (uint)PLDActionID.预警)
                {
                    string _spellName = spell.Name;
                    if (SC.GetSC("极致防御"))
                    {
                        LogHelper.Print("技能使用", $"{_spellName} 已成功对{spell.GetTarget().Name}使用,距离激活{(DateTime.Now - SC.GetSCActivationTime("极致防御")).TotalSeconds:F2}秒");
                        SC.SetSC("极致防御", false);
                        settings.SetDefaultSC("极致防御");
                    }
                    else
                    {
                        LogHelper.Print("技能使用", $"{_spellName} 已成功对{spell.GetTarget().Name}使用，使用了不推荐的方式，推荐使用acr中的技能使用来使用技能");
                    }
                }
                else if (spell.Id == (uint)PLDActionID.壁垒)
                {
                    string _spellName = spell.Name;
                    if (SC.GetSC(_spellName))
                    {
                        LogHelper.Print("技能使用", $"{_spellName} 已成功对{spell.GetTarget().Name}使用,距离激活{(DateTime.Now - SC.GetSCActivationTime(_spellName)).TotalSeconds:F2}秒");
                        SC.SetSC(_spellName, false);
                        settings.SetDefaultSC(_spellName);
                    }
                    else
                    {
                        LogHelper.Print("技能使用", $"{_spellName} 已成功对{spell.GetTarget().Name}使用，使用了不推荐的方式，推荐使用acr中的技能使用来使用技能");
                    }
                }
                else if (spell.Id == (uint)PLDActionID.圣盾阵)
                {
                    string _spellName = spell.Name;
                    if (SC.GetSC(_spellName))
                    {
                        LogHelper.Print("技能使用", $"{_spellName} 已成功对{spell.GetTarget().Name}使用,距离激活{(DateTime.Now - SC.GetSCActivationTime(_spellName)).TotalSeconds:F2}秒");
                        SC.SetSC(_spellName, false);
                        settings.SetDefaultSC(_spellName);
                    }
                    else
                    {
                        LogHelper.Print("技能使用", $"{_spellName} 已成功对{spell.GetTarget().Name}使用，使用了不推荐的方式，推荐使用acr中的技能使用来使用技能");
                    }
                }
                else if (spell.Id == (uint)PLDActionID.盾阵)
                {
                    string _spellName = spell.Name;
                    if (SC.GetSC(_spellName))
                    {
                        LogHelper.Print("技能使用", $"{_spellName} 已成功对{spell.GetTarget().Name}使用,距离激活{(DateTime.Now - SC.GetSCActivationTime("圣盾阵")).TotalSeconds:F2}秒");
                        SC.SetSC("圣盾阵", false);
                        settings.SetDefaultSC("圣盾阵");
                    }
                    else
                    {
                        LogHelper.Print("技能使用", $"{_spellName} 已成功对{spell.GetTarget().Name}使用，使用了不推荐的方式，推荐使用acr中的技能使用来使用技能");
                    }
                }
                else if (spell.Id == (uint)PLDActionID.神圣领域)
                {
                    string _spellName = spell.Name;
                    if (SC.GetSC(_spellName))
                    {
                        LogHelper.Print("技能使用", $"{_spellName} 已成功对{spell.GetTarget().Name}使用,距离激活{(DateTime.Now - SC.GetSCActivationTime(_spellName)).TotalSeconds:F2}秒");
                        SC.SetSC(_spellName, false);
                        settings.SetDefaultSC(_spellName);
                    }
                    else
                    {
                        LogHelper.Print("技能使用", $"{_spellName} 已成功对{spell.GetTarget().Name}使用，使用了不推荐的方式，推荐使用acr中的技能使用来使用技能");
                    }
                }
                else if (spell.Id == (uint)PLDActionID.圣光幕帘)
                {
                    string _spellName = spell.Name;
                    if (SC.GetSC(_spellName))
                    {
                        LogHelper.Print("技能使用", $"{_spellName} 已成功对{spell.GetTarget().Name}使用,距离激活{(DateTime.Now - SC.GetSCActivationTime(_spellName)).TotalSeconds:F2}秒");
                        SC.SetSC(_spellName, false);
                        settings.SetDefaultSC(_spellName);
                    }
                    else
                    {
                        LogHelper.Print("技能使用", $"{_spellName} 已成功对{spell.GetTarget().Name}使用，使用了不推荐的方式，推荐使用acr中的技能使用来使用技能");
                    }
                }
                else if (spell.Id == (uint)PLDActionID.铁壁)
                {
                    string _spellName = spell.Name;
                    if (SC.GetSC(_spellName))
                    {
                        LogHelper.Print("技能使用", $"{_spellName} 已成功对{spell.GetTarget().Name}使用,距离激活{(DateTime.Now - SC.GetSCActivationTime(_spellName)).TotalSeconds:F2}秒");
                        SC.SetSC(_spellName, false);
                        settings.SetDefaultSC(_spellName);
                    }
                    else
                    {
                        LogHelper.Print("技能使用", $"{_spellName} 已成功对{spell.GetTarget().Name}使用，使用了不推荐的方式，推荐使用acr中的技能使用来使用技能");
                    }
                }
                else if (spell.Id == (uint)PLDActionID.武装戍卫)
                {
                    string _spellName = spell.Name;
                    if (SC.GetSC(_spellName))
                    {
                        LogHelper.Print("技能使用", $"{_spellName} 已成功对{spell.GetTarget().Name}使用,距离激活{(DateTime.Now - SC.GetSCActivationTime(_spellName)).TotalSeconds:F2}秒");
                        SC.SetSC(_spellName, false);
                        settings.SetDefaultSC(_spellName);
                    }
                    else
                    {
                        LogHelper.Print("技能使用", $"{_spellName} 已成功对{spell.GetTarget().Name}使用，使用了不推荐的方式，推荐使用acr中的技能使用来使用技能");
                    }
                }
                else if (spell.Id == (uint)PLDActionID.雪仇)
                {
                    string _spellName = spell.Name;
                    if (SC.GetSC(_spellName))
                    {
                        LogHelper.Print("技能使用", $"{_spellName} 已成功对{spell.GetTarget().Name}使用,距离激活{(DateTime.Now - SC.GetSCActivationTime(_spellName)).TotalSeconds:F2}秒");
                        SC.SetSC(_spellName, false);
                        settings.SetDefaultSC(_spellName);
                    }
                    else
                    {
                        LogHelper.Print("技能使用", $"{_spellName} 已成功对{spell.GetTarget().Name}使用，使用了不推荐的方式，推荐使用acr中的技能使用来使用技能");
                    }
                }
                else if (spell.Id == (uint)PLDActionID.亲疏自行)
                {
                    string _spellName = spell.Name;
                    if (SC.GetSC(_spellName))
                    {
                        LogHelper.Print("技能使用", $"{_spellName} 已成功对{spell.GetTarget().Name}使用,距离激活{(DateTime.Now - SC.GetSCActivationTime(_spellName)).TotalSeconds:F2}秒");
                        SC.SetSC(_spellName, false);
                        settings.SetDefaultSC(_spellName);
                    }
                    else
                    {
                        LogHelper.Print("技能使用", $"{_spellName} 已成功对{spell.GetTarget().Name}使用，使用了不推荐的方式，推荐使用acr中的技能使用来使用技能");
                    }
                }
                else if (spell.Id == (uint)PLDActionID.调停)
                {
                    string _spellName = spell.Name;
                    if (SC.GetSC(_spellName))
                    {
                        LogHelper.Print("技能使用", $"{_spellName} 已成功对{spell.GetTarget().Name}使用,距离激活{(DateTime.Now - SC.GetSCActivationTime(_spellName)).TotalSeconds:F2}秒");
                        SC.SetSC(_spellName, false);
                        settings.SetDefaultSC(_spellName);
                    }
                }
                else if (spell.Id == (uint)PLDActionID.下踢)
                {
                    string _spellName = spell.Name;
                    if (SC.GetSC(_spellName))
                    {
                        LogHelper.Print("技能使用", $"{_spellName} 已成功对{spell.GetTarget().Name}使用,距离激活{(DateTime.Now - SC.GetSCActivationTime(_spellName)).TotalSeconds:F2}秒");
                        SC.SetSC(_spellName, false);
                        settings.SetDefaultSC(_spellName);
                    }
                    else
                    {
                        LogHelper.Print("技能使用", $"{_spellName} 已成功对{spell.GetTarget().Name}使用，使用了不推荐的方式，推荐使用acr中的技能使用来使用技能");
                    }
                }
                else if (spell.Id == (uint)PLDActionID.插言)
                {
                    string _spellName = spell.Name;
                    if (SC.GetSC(_spellName))
                    {
                        LogHelper.Print("技能使用", $"{_spellName} 已成功对{spell.GetTarget().Name}使用,距离激活{(DateTime.Now - SC.GetSCActivationTime(_spellName)).TotalSeconds:F2}秒");
                        SC.SetSC(_spellName, false);
                        settings.SetDefaultSC(_spellName);
                    }
                    else
                    {
                        LogHelper.Print("技能使用", $"{_spellName} 已成功对{spell.GetTarget().Name}使用，使用了不推荐的方式，推荐使用acr中的技能使用来使用技能");
                    }
                }
                else if (spell.Id == (uint)PLDActionID.挑衅)
                {
                    string _spellName = spell.Name;
                    if (SC.GetSC(_spellName))
                    {
                        LogHelper.Print("技能使用", $"{_spellName} 已成功对{spell.GetTarget().Name}使用,距离激活{(DateTime.Now - SC.GetSCActivationTime(_spellName)).TotalSeconds:F2}秒");
                        SC.SetSC(_spellName, false);
                        settings.SetDefaultSC(_spellName);
                    }
                    else
                    {
                        LogHelper.Print("技能使用", $"{_spellName} 已成功对{spell.GetTarget().Name}使用，使用了不推荐的方式，推荐使用acr中的技能使用来使用技能");
                    }
                }
                else if (spell.Id == (uint)PLDActionID.退避)
                {
                    string _spellName = spell.Name;
                    if (SC.GetSC(_spellName))
                    {
                        LogHelper.Print("技能使用", $"{_spellName} 已成功对{spell.GetTarget().Name}使用,距离激活{(DateTime.Now - SC.GetSCActivationTime(_spellName)).TotalSeconds:F2}秒");
                        SC.SetSC(_spellName, false);
                        settings.SetDefaultSC(_spellName);
                    }
                    else
                    {
                        LogHelper.Print("技能使用", $"{_spellName} 已成功对{spell.GetTarget().Name}使用，使用了不推荐的方式，推荐使用acr中的技能使用来使用技能");
                    }
                }
                else if (spell.Id == (uint)PLDActionID.干预)
                {
                    string _spellName = spell.Name;
                    if (SC.GetSC(_spellName))
                    {
                        LogHelper.Print("技能使用", $"{_spellName} 已成功对{spell.GetTarget().Name}使用,距离激活{(DateTime.Now - SC.GetSCActivationTime(_spellName)).TotalSeconds:F2}秒");
                        SC.SetSC(_spellName, false);
                        settings.SetDefaultSC(_spellName);
                    }
                    else
                    {
                        LogHelper.Print("技能使用", $"{_spellName} 已成功对{spell.GetTarget().Name}使用，使用了不推荐的方式，推荐使用acr中的技能使用来使用技能");
                    }
                }
                else if (spell.Id == (uint)PLDActionID.保护)
                {
                    string _spellName = spell.Name;
                    if (SC.GetSC(_spellName))
                    {
                        LogHelper.Print("技能使用", $"{_spellName} 已成功对{spell.GetTarget().Name}使用,距离激活{(DateTime.Now - SC.GetSCActivationTime(_spellName)).TotalSeconds:F2}秒");
                        SC.SetSC(_spellName, false);
                        settings.SetDefaultSC(_spellName);
                    }
                    else
                    {
                        LogHelper.Print("技能使用", $"{_spellName} 已成功对{spell.GetTarget().Name}使用，使用了不推荐的方式，推荐使用acr中的技能使用来使用技能");
                    }
                }
                else if (spell.Id == (uint)PLDActionID.深仁厚泽)
                {
                    string _spellName = spell.Name;
                    if (SC.GetSC(_spellName))
                    {
                        LogHelper.Print("技能使用", $"{_spellName} 已成功对{spell.GetTarget().Name}使用,距离激活{(DateTime.Now - SC.GetSCActivationTime(_spellName)).TotalSeconds:F2}秒");
                        SC.SetSC(_spellName, false);
                        settings.SetDefaultSC(_spellName);
                    }
                    else
                    {
                        LogHelper.Print("技能使用", $"{_spellName} 已成功对{spell.GetTarget().Name}使用，使用了不推荐的方式，推荐使用acr中的技能使用来使用技能");
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
            //Frost_PLD_BattleData.Instance.战斗开始时长 = currTimeInMs;
            Frost_PLD_BattleData.Instance.战斗开始时长 = AI.Instance.BattleData.CurrBattleTimeInMs;
            Frost_PLD_BattleData.Instance.战斗开始分钟数 = (uint)Math.Floor((double)Frost_PLD_BattleData.Instance.战斗开始时长 / 60000) % 60;


        }

        public static IFramework Framework => ECHelper.Framework;
        public async void OnEnterRotation()
        {
            LogHelper.Print("ACR提示", "已切换到Frost的骑士acr"); ;
            Frost_PLD_Settings.Instance.LoadQt();
            try
            {
                Svc.Commands.RemoveHandler("/Frost_PLD");
            }
            catch (Exception)
            {
            }
            // 注册命令
            Svc.Commands.AddHandler("/Frost_PLD", new CommandInfo(NewCommandHandler)
            {
                HelpMessage = "Frost的骑士acr控制命令"
            });
            LogHelper.Print("ACR提示", "Frost_PLD命令已注册,推荐使用命令来开启减伤技能");// 假设 verifyRequestInstance 是现有的 AEAssist.Verify.VerifyRequest 对象
            Framework.Update += UpdateACR;
            LogHelper.Print("ACR提示", "Framework.Update已注册");
            await Utilities.UpdateTriggerlines("PLD");
        }
        public static void UpdateACR(IFramework framework)
        {
            //Do something
            Frost_PLD_UpdateACRData.UpdateBattleData();
            Frost_PLD_UpdateACRData.ModifySlotResolvers(Frost_PLD_RotationEntry.SlotResolvers);
        }
        private static void NewCommandHandler(string command, string args)
        {
            var QT = Frost_PLD_RotationEntry.JobViewWindow;


        }

        public void OnExitRotation()
        {
            Frost_PLD_Settings.Instance.SaveQt();
            // 注销命令
            Svc.Commands.RemoveHandler("/Frost_PLD");
            LogHelper.Print("ACR提示", "Frost_PLD命令已注销");
            Framework.Update -= UpdateACR;
            LogHelper.Print("ACR提示", "Framework.Update已注销");
        }

        public async void OnTerritoryChanged()
        {
            DutyInfo dutyInfo = new DutyInfo();
            if (dutyInfo.InDuty)
            {
                Frost_PLD_RotationEntry.Frost_PLD_ArcUI.AddLog($"已进入副本{dutyInfo.Id}:{dutyInfo.Name},该副本人数为{dutyInfo.MemberNum}");
            }
        }

    }
}
