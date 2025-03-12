using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.CombatRoutine.View.JobView.HotkeyResolver;
using AEAssist.CombatRoutine.View.JobView;
using Frost.Frost_PLD.Frost_PLD_Setting;
using Frost.Frost_PLD.Frost_PLD_SlotResolvers;
using Frost.Frost_PLD.Frost_PLD_Triggers;
using Frost.Frost_PLD;
using Frost.HOOK;
using AEAssist.CombatRoutine.Module.Opener;
using System.Xml.Linq;
using AEAssist.Command;
using Dalamud.Game.Command;
using AEAssist.Helper;
using Dalamud.Plugin;
using static Dalamud.Interface.Utility.Raii.ImRaii;
using ScriptTest;
using AEAssist.MemoryApi;
using AEAssist;
using Dalamud.Plugin.Services;
using AEAssist.Extension;
using Frost.Common;
using Dalamud.Interface.Windowing;
namespace Frost;

public class Frost_PLD_RotationEntry : IRotationEntry
{

    public string AuthorName { get; set; } = "Frost";

    public Rotation Build(string settingFolder)
    {
        // 初始化设置
        Frost_PLD_Settings.Build(settingFolder);
        // 初始化QT （依赖了设置的数据）
        BuildQT();
        BuildSC();

        var rot = new Rotation(SlotResolvers)
        {
            TargetJob = Jobs.Paladin,
            AcrType = AcrType.HighEnd,
            MinLevel = 100,
            MaxLevel = 100,
            Description = "目前不可用",
        };
        //添加起手
        rot.AddOpener(GetOpener);
        // 添加各种事件回调
        rot.SetRotationEventHandler(new Frost_PLD_RotationEventHandler());
        // 添加QT开关的时间轴行为
        rot.AddTriggerAction(new Frost_PLD_TriggerAction_QT(), new Frost_PLD_TriggerAction_SpellCast());
        rot.AddTriggerCondition();

        LogHelper.Info("Frost_PLD_RotationEntry初始化完成");
        return rot;
    }
    public static bool 时间轴自动跟随启用情况 = false;
    public static bool 时间轴自动跟随启用启用情况 = false;
    // 声明当前要使用的UI的实例 示例里使用QT
    public static JobViewWindow JobViewWindow
    {
        get;
        private set;
    }

    public static SCWindow scWindow
    {
        get;
        set;
    }


    public static Frost_PLD_AcrUi Frost_PLD_ArcUI = new();

    // 构造函数里初始化QT
    public void BuildQT()
    {
        // JobViewSave是AE底层提供的QT设置存档类 在你自己的设置里定义即可
        // 第二个参数是你设置文件的Save类 第三个参数是QT窗口标题
        Frost_PLD_RotationEntry.JobViewWindow = new JobViewWindow(Frost_PLD_Settings.Instance.JobViewSave, Frost_PLD_Settings.Instance.Save, "Frost 骑士");
        //Frost_PLD_RotationEntry.JobViewWindow.SetUpdateAction(OnUIUpdate); // 设置QT中的Update回调 不需要就不设置

        //添加QT分页 第一个参数是分页标题 第二个是分页里的内容
        Frost_PLD_RotationEntry.JobViewWindow.AddTab("通用", new Action<JobViewWindow>(Frost_PLD_ArcUI.DrawGeneral));
        Frost_PLD_RotationEntry.JobViewWindow.AddTab("Dev", new Action<JobViewWindow>(Frost_PLD_ArcUI.DrawDev));
        Frost_PLD_RotationEntry.JobViewWindow.AddTab("更新说明与反馈", new Action<JobViewWindow>(Frost_PLD_ArcUI.DrawUpdate));

        void AddQt(string name, bool qtValueDefault, string toolTip)
        {
            Action<bool> action = (value) =>
            {
                Frost_PLD_RotationEntry.Frost_PLD_ArcUI.AddLog($"QT被点击: {name} = {value}");
                if (name == "突进无位移")
                {
                    if (value == false)
                    {
                        Hook.DisablePatch(PatchType.NoActionMove);
                        LogHelper.Print("关闭突进无位移");
                    }
                    else
                    {
                        Hook.EnablePatch(PatchType.NoActionMove);
                        LogHelper.Print("开启突进无位移");
                    }
                }
                if (name == "后摇可移动")
                {
                    if (value == false)
                    {
                        Hook.DisablePatch(PatchType.NoActionMove);
                        LogHelper.Print("关闭后摇可移动");
                    }
                    else
                    {
                        Hook.EnablePatch(PatchType.SkillPostActionMove);
                        LogHelper.Print("开启后摇可移动");
                    }
                }
                if (name == "长臂猿")
                {
                    if (value == false)
                    {
                        Hook.DisablePatch(PatchType.ActionRange);
                        LogHelper.Print("关闭长臂猿");
                    }
                    else
                    {
                        Hook.EnablePatch(PatchType.ActionRange);
                        LogHelper.Print("开启长臂猿");
                    }
                }
            };
            Frost_PLD_RotationEntry.JobViewWindow.AddQt(name, qtValueDefault, action);
            Frost_PLD_RotationEntry.JobViewWindow.SetQtToolTip(toolTip);
        }

        // 添加QT开关 第二个参数是默认值 (开or关) 第三个参数是鼠标悬浮时的tips
        AddQt("突进无位移", false, "突进不位移");
        AddQt("后摇可移动", false, "技能后摇可以移动");
        AddQt("长臂猿", false, "增加3的技能距离");
        AddQt("停手", false, "开启后不再使用技能");
        AddQt("盾姿", true, "开启后将会开启盾姿");
        AddQt("1仇", false, "开启后会尽可能保持1仇");
        AddQt("打完调停", false, "开启后会打完调停层数");
        AddQt("允许突进", false, "开启后允许调停突进");
        // 添加快捷按钮 (带技能图标)
        Frost_PLD_RotationEntry.JobViewWindow.AddHotkey("爆发药", new HotKeyResolver_Potion());
        Frost_PLD_RotationEntry.JobViewWindow.AddHotkey("极限技", new HotKeyResolver_LB());

        /*
        // 这是一个自定义的快捷按钮 一般用不到
        // 图片路径是相对路径 基于AEAssist(C|E)NVersion/AEAssist
        // 如果想用AE自带的图片资源 路径示例: Resources/AE2Logo.png
        SageRotationEntry.QT.AddHotkey("极限技", new HotkeyResolver_General("#自定义图片路径", () =>
        {
            // 点击这个图片会触发什么行为
            LogHelper.Print("你好");
        }));
        */
    }


    // 构造函数里初始化QT
    public void BuildSC()
    {
        Frost_PLD_Settings Setting = Frost_PLD_Settings.Instance;
        scWindow= new SCWindow();
        // 添加按钮
        scWindow.AddSC("钢铁信念", Setting.cd预检测阈值, false, TargetType.自身);
        scWindow.AddSC("冲刺", Setting.cd预检测阈值, false, TargetType.自身);
        scWindow.AddSC("极致防御", Setting.cd预检测阈值, false, TargetType.自身);
        scWindow.AddSC("壁垒", Setting.cd预检测阈值, false, TargetType.自身);
        scWindow.AddSC("圣盾阵", Setting.cd预检测阈值, false, TargetType.自身);
        scWindow.AddSC("神圣领域", Setting.cd预检测阈值, false, TargetType.自身);
        scWindow.AddSC("圣光幕帘", Setting.cd预检测阈值, false, TargetType.自身);
        scWindow.AddSC("铁壁", Setting.cd预检测阈值, false, TargetType.自身);
        scWindow.AddSC("武装戍卫", Setting.cd预检测阈值, false, TargetType.自身);
        scWindow.AddSC("雪仇", Setting.cd预检测阈值, false, TargetType.自身);
        scWindow.AddSC("亲疏自行", Setting.cd预检测阈值, false, TargetType.自身);
        scWindow.AddSC("调停", Setting.cd预检测阈值, false, TargetType.当前目标);
        scWindow.AddSC("下踢", Setting.cd预检测阈值, false, TargetType.当前目标);
        scWindow.AddSC("插言", Setting.cd预检测阈值, false, TargetType.当前目标);
        scWindow.AddSC("挑衅", Setting.cd预检测阈值, false, TargetType.当前目标);
        scWindow.AddSC("退避", Setting.cd预检测阈值, false, TargetType.搭档);
        scWindow.AddSC("干预", Setting.cd预检测阈值, false, TargetType.搭档);
        scWindow.AddSC("保护", Setting.cd预检测阈值, false, TargetType.血量最低队友);
        scWindow.AddSC("深仁厚泽", Setting.cd预检测阈值, false, TargetType.血量最低队友);

        // 在Update中调用绘制方法

        JobViewWindow.SetUpdateAction(() =>
        {
            scWindow.Draw();
        });


    }

    // 逻辑从上到下判断，通用队列是无论如何都会判断的 
    // gcd则在可以使用gcd时判断
    // offGcd则在不可以使用gcd 且没达到gcd内插入能力技上限时判断
    // pvp环境下 全都强制认为是通用队列
    public static List<SlotResolverData> SlotResolvers { get; set; } = new()
    {
        // 通用队列 不管是不是gcd 都会判断的逻辑
        //new(new XXXXXXXX(),SlotMode.Always),
        new(new inChecking(),SlotMode.Always),
        //以下是前置
        new(new 前置_开盾(),SlotMode.Always),
        new(new 前置_关盾(),SlotMode.Always),

        //以下是技能使用
        new(new 技能使用_冲刺(),SlotMode.Always),
        new(new 技能使用_钢铁信念(),SlotMode.Always),

        //以下是GCD        
        new(new BOSS中_战逃葬送剑即将过期(),SlotMode.Always),
        new(new BOSS中_战逃祈告剑即将过期(),SlotMode.Always),
        new(new BOSS中_战逃强化圣灵即将过期(),SlotMode.Always),
        new(new BOSS中_战逃赎罪剑即将过期(),SlotMode.Always),
        new(new BOSS中_沥血剑(),SlotMode.Always),
        new(new BOSS中_英勇之剑(),SlotMode.Always),
        new(new BOSS中_真理之剑(),SlotMode.Always),
        new(new BOSS中_信念之剑(),SlotMode.Always),
        new(new BOSS中_悔罪(),SlotMode.Always),
        new(new BOSS中_战逃赎罪剑三连(),SlotMode.Always),
        new(new BOSS中_战逃葬送剑(),SlotMode.Always),
        new(new BOSS中_战逃祈告剑(),SlotMode.Always),
        new(new BOSS中_战逃强化圣灵(),SlotMode.Always),
        new(new BOSS中_战逃赎罪剑(),SlotMode.Always),
        new(new BOSS中_战逃王权剑(),SlotMode.Always),
        new(new BOSS中_战逃战女神之怒(),SlotMode.Always),
        new(new BOSS中_战逃暴乱剑(),SlotMode.Always),
        new(new BOSS中_战逃先锋剑(),SlotMode.Always),


        new(new BOSS中_战逃外葬送剑即将过期(),SlotMode.Always),
        new(new BOSS中_战逃外祈告剑即将过期(),SlotMode.Always),
        new(new BOSS中_战逃外强化圣灵即将过期(),SlotMode.Always),
        new(new BOSS中_战逃外赎罪剑即将过期(),SlotMode.Always),
        new(new BOSS中_战逃外赎罪剑让强化圣灵(),SlotMode.Always),
        new(new BOSS中_战逃外先锋剑(),SlotMode.Always),
        new(new BOSS中_战逃外暴乱剑(),SlotMode.Always),
        new(new BOSS中_战逃外战女神之怒(),SlotMode.Always),
        new(new BOSS中_战逃外王权剑(),SlotMode.Always),
        new(new BOSS中_战逃外赎罪剑(),SlotMode.Always),
        new(new BOSS中_战逃外祈告剑(),SlotMode.Always),
        new(new BOSS中_战逃外强化圣灵(),SlotMode.Always),
        new(new BOSS中_战逃外葬送剑(),SlotMode.Always),

        
        //以下是OGCD
        new(new BOSS战斗中_战逃反应(),SlotMode.Always),
        new(new BOSS战斗中_战逃绝对统治(),SlotMode.Always),
        new(new BOSS战斗中_战逃安魂祈祷(),SlotMode.Always),
        new(new BOSS战斗中_荣耀之剑(),SlotMode.Always),
        new(new BOSS战斗中_战逃厄运流转(),SlotMode.Always),
        new(new BOSS战斗中_战逃偿赎剑(),SlotMode.Always),
        new(new BOSS战斗中_战逃深奥之灵(),SlotMode.Always),
        new(new BOSS战斗中_战逃调停(),SlotMode.Always),
        new(new BOSS战斗中_战逃外厄运流转(),SlotMode.Always),
        new(new BOSS战斗中_战逃外偿赎剑(),SlotMode.Always),
        new(new BOSS战斗中_战逃外深奥之灵(),SlotMode.Always),

    };

    private IOpener? GetOpener(uint level)//设置起手
    {
        return new PLD_100_Opener();
    }
    // 设置界面
    public void OnDrawSetting()
    {
        Frost_PLD_SettingUI.Instance.Draw();
    }
    private void IRotationUI()
    {
    }

    // Token: 0x06000011 RID: 17 RVA: 0x0000223B File Offset: 0x0000043B
    public IRotationUI GetRotationUI()
    {
        return Frost_PLD_RotationEntry.JobViewWindow;
    }
    public void Dispose()
    {
        // 释放需要释放的东西 没有就留空
        Frost_PLD_RotationEventHandler.Framework.Update -= Frost_PLD_RotationEventHandler.UpdateACR;
    }
}


