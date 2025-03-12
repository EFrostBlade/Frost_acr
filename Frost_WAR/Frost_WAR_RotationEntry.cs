using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.CombatRoutine.View.JobView.HotkeyResolver;
using AEAssist.CombatRoutine.View.JobView;
using Frost.Frost_WAR.Frost_WAR_Setting;
using Frost.Frost_WAR.Frost_WAR_SlotResolvers;
using Frost.Frost_WAR.Frost_WAR_Triggers;
using Frost.Frost_WAR;
using AEAssist.CombatRoutine.Module.Opener;
using System.Xml.Linq;
using AEAssist.Command;
using Dalamud.Game.Command;
using AEAssist.Helper;
using Dalamud.Plugin;
using ECommons.DalamudServices;
using static Dalamud.Interface.Utility.Raii.ImRaii;
using ScriptTest;
using AEAssist.MemoryApi;
using AEAssist;
using Dalamud.Plugin.Services;
using AEAssist.Extension;
using Frost.Frost_WAR.Frost_WAR_Data;
using Frost.HOOK;
namespace Frost;

public class Frost_WAR_RotationEntry : IRotationEntry
{

    public string AuthorName { get; set; } = "Frost";

    public Rotation Build(string settingFolder)
    {
        // 初始化设置
        Frost_WAR_Settings.Build(settingFolder);
        // 初始化QT （依赖了设置的数据）
        BuildQT();

        var rot = new Rotation(SlotResolvers)
        {
            TargetJob = Jobs.Warrior,
            AcrType = AcrType.Both,
            MinLevel = 0,
            MaxLevel = 100,
            Description = "战士全等级日随可用,自动拉怪减伤\n高难也可使用",
        };
        //添加起手
        rot.AddOpener(GetOpener);
        // 添加各种事件回调
        rot.SetRotationEventHandler(new Frost_WAR_RotationEventHandler());
        // 添加QT开关的时间轴行为
        rot.AddTriggerAction(new Frost_WAR_TriggerAction_QT(), new 移动到目标点(), new 自动减伤(), new 自动保留尽毁(),new 启用qt控制盾姿(),new 停止跟随());
        rot.AddTriggerCondition(new 检测目标位置(), new 检测自身位置(), new 对位挑衅(), new 自己接仇(), new 拉怪到目标位置(),new 跟随目标());
        
        LogHelper.Info("Frost_WAR_RotationEntry初始化完成");
        return rot;
    }
    public static bool 时间轴自动跟随启用情况=false;
    public static bool 时间轴自动跟随启用启用情况 = false;
    // 声明当前要使用的UI的实例 示例里使用QT
    public static JobViewWindow JobViewWindow
    {
        get;
        private set;
    }

    public static Frost_WAR_AcrUi Frost_WAR_ArcUI = new();

    // 构造函数里初始化QT
    public void BuildQT()
    {
        // JobViewSave是AE底层提供的QT设置存档类 在你自己的设置里定义即可
        // 第二个参数是你设置文件的Save类 第三个参数是QT窗口标题
        Frost_WAR_RotationEntry.JobViewWindow = new JobViewWindow(Frost_WAR_Settings.Instance.JobViewSave, Frost_WAR_Settings.Instance.Save, "Frost 战士");
        //Frost_WAR_RotationEntry.JobViewWindow.SetUpdateAction(OnUIUpdate); // 设置QT中的Update回调 不需要就不设置

        //添加QT分页 第一个参数是分页标题 第二个是分页里的内容
        Frost_WAR_RotationEntry.JobViewWindow.AddTab("通用", new Action<JobViewWindow>(Frost_WAR_ArcUI.DrawGeneral));
        Frost_WAR_RotationEntry.JobViewWindow.AddTab("Dev", new Action<JobViewWindow>(Frost_WAR_ArcUI.DrawDev));
        Frost_WAR_RotationEntry.JobViewWindow.AddTab("更新说明与反馈", new Action<JobViewWindow>(Frost_WAR_ArcUI.DrawUpdate));

        void AddQt(string name, bool qtValueDefault, string toolTip)
        {
            Action<bool> action = (value) =>
            {
                Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog($"QT被点击: {name} = {value}");
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
                if(name== "后摇可移动")
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
                if (name == "盾姿")
                {
                    if (Frost_WAR_Settings.Instance.启用qt控制盾姿)
                    {
                        bool isShieldActive = GameObjectExtension.HasAura(Core.Me, (uint)WARBuffID.守护);
                        bool shouldActivateShield = Frost_WAR_RotationEntry.JobViewWindow.GetQt("盾姿");
                        bool isShieldColldown = SpellHelper.GetSpell((uint)WARActionID.守护).Cooldown.TotalSeconds == 0 && SpellHelper.GetSpell((uint)WARActionID.解除守护).Cooldown.TotalSeconds == 0;

                        if (isShieldColldown && shouldActivateShield && !isShieldActive && !SpellHelper.GetSpell((uint)WARActionID.守护).RecentlyUsed() && !SpellHelper.GetSpell((uint)WARActionID.解除守护).RecentlyUsed())
                        {
                            Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("战斗外开盾");
                            SpellHelper.GetSpell((uint)WARActionID.守护).Cast();
                        }
                        else if (isShieldColldown && !shouldActivateShield && isShieldActive && !SpellHelper.GetSpell((uint)WARActionID.解除守护).RecentlyUsed() && !SpellHelper.GetSpell((uint)WARActionID.守护).RecentlyUsed())
                        {
                            Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("战斗外关盾");
                            SpellHelper.GetSpell((uint)WARActionID.解除守护).Cast();
                        }
                    }
                }
            };
            Frost_WAR_RotationEntry.JobViewWindow.AddQt(name, qtValueDefault, action);
            Frost_WAR_RotationEntry.JobViewWindow.SetQtToolTip(toolTip);
        }

        // 添加QT开关 第二个参数是默认值 (开or关) 第三个参数是鼠标悬浮时的tips
        AddQt("突进无位移", false, "突进不位移");
        AddQt("后摇可移动", false, "技能后摇可以移动");
        AddQt("立刻血气", false, "立刻开启原初的血气/原初的直觉");
        AddQt("立刻勇猛", false, "立刻对另一个t使用原初的勇猛");
        AddQt("立刻雪仇", false, "立刻使用雪仇");
        AddQt("立刻铁壁", false, "立刻开启铁壁");
        AddQt("立刻戮罪", false, "立刻开启复仇/戮罪");
        AddQt("立刻战栗", false, "立刻开启战栗");
        AddQt("立刻泰然", false, "立刻开启泰然自若");
        AddQt("立刻摆脱", false, "立刻开启摆脱");
        AddQt("立刻防击退", false, "立刻开启亲疏自行");
        AddQt("强制突进", false, "立刻强制使用猛攻突进");
        AddQt("立刻突进", false, "立刻使用猛攻突进");
        AddQt("立刻冲刺", false, "立刻使用冲刺疾跑");
        AddQt("立刻爆发药", false, "立刻使用设置的爆发药");
        AddQt("立刻挑衅", false, "立刻使用挑衅（更推荐使用1仇qt来控制挑衅）");
        AddQt("停手", false, "开启后不再使用技能");
        AddQt("盾姿", true, "开启后将会开启盾姿");
        AddQt("1仇", false, "开启后会尽可能保持1仇");
        AddQt("奶人", false, "开启后将会使用勇猛奶队友");
        AddQt("爆发药", false, "开启后将会在合适的时机使用爆发药");
        AddQt("爆发药不对团辅", false, "开启后将会尽可能早地使用爆发药");
        AddQt("前置尽毁", false, "解放开启后先打蛮荒崩裂和尽毁");
        AddQt("保留尽毁", false, "保留蛮荒、尽毁和一定兽魂直到爆发");
        AddQt("打完猛攻", false, "开启后将在合适的时机打空猛攻层数");
        AddQt("允许突进", false, "开启后猛攻和蛮荒可以在目标圈外及移动中使用");
        AddQt("禁用aoe", false, "开启后不会使用任何aoe技能");
        AddQt("禁用飞斧", false, "开启后即使远离目标也不会使用飞斧");
        AddQt("双目标aoe", false, "开启后在双目标时也使用aoe技能");
        AddQt("泄资源", false, "开启后将尽快打空所有资源");
        AddQt("没红斩也泄", false, "开启后将无视红斩buff尽快打空所有资源");
        AddQt("立刻死斗", false, "立刻使用死斗");
        AddQt("立刻退避", false, "立刻退避一个合适的目标");
        AddQt("立刻勇猛远敏", false, "立刻对远敏使用原初的勇猛");
        AddQt("立刻勇猛法系", false, "立刻对法系使用原初的勇猛");
        void AddToUnVisibleList(string item)
        {
            if (!Frost_WAR_Settings.Instance.JobViewSave.QtUnVisibleList.Contains(item))
            {
                Frost_WAR_Settings.Instance.JobViewSave.QtUnVisibleList.Add(item);
            }
        }

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
            AddToUnVisibleList("后摇可移动");
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
            AddToUnVisibleList("后摇可移动");
        }
        if (!Frost_WAR_Settings.Instance.启用qt控制盾姿)
        {
            if (!Frost_WAR_Settings.Instance.JobViewSave.QtUnVisibleList.Contains("盾姿"))
            {
                Frost_WAR_Settings.Instance.JobViewSave.QtUnVisibleList.Add("盾姿");
            }
        }
        else
        {
            Frost_WAR_Settings.Instance.JobViewSave.QtUnVisibleList.Remove("启用qt控制盾姿");
        }
        // 添加快捷按钮 (带技能图标)
        Frost_WAR_RotationEntry.JobViewWindow.AddHotkey("爆发药", new HotKeyResolver_Potion());
        Frost_WAR_RotationEntry.JobViewWindow.AddHotkey("极限技", new HotKeyResolver_LB());

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
    // 逻辑从上到下判断，通用队列是无论如何都会判断的 
    // gcd则在可以使用gcd时判断
    // offGcd则在不可以使用gcd 且没达到gcd内插入能力技上限时判断
    // pvp环境下 全都强制认为是通用队列
    public static List<SlotResolverData> SlotResolvers { get; set; } = new()
    {
        // 通用队列 不管是不是gcd 都会判断的逻辑
        //new(new XXXXXXXX(),SlotMode.Always),
        new(new inChecking(),SlotMode.Always),
        //以下是GCD
        new(new 泄资源_尽毁(),SlotMode.Always),
        new(new 泄资源_蛮荒崩裂(),SlotMode.Always),
        new(new 泄资源_混沌旋风(),SlotMode.Always),
        new(new 泄资源_狂魂(),SlotMode.Always),
        new(new 泄资源_钢铁旋风(),SlotMode.Always),
        new(new 泄资源_原初之魂(),SlotMode.Always),
        new(new 泄资源_秘银暴风(),SlotMode.Always),
        new(new 泄资源_超压斧(),SlotMode.Always),
        new(new 泄资源_暴风碎(),SlotMode.Always),
        new(new 泄资源_暴风斩(),SlotMode.Always),
        new(new 泄资源_凶残裂(),SlotMode.Always),
        new(new 泄资源_重劈(),SlotMode.Always),
        new(new 泄资源_飞斧(),SlotMode.Always),
        new(new BOSS中_尽毁(),SlotMode.Always),
        new(new BOSS中_保留尽毁(),SlotMode.Always),
        new(new BOSS中_蛮荒崩裂(),SlotMode.Always),
        new(new BOSS中_保留蛮荒(),SlotMode.Always),
        new(new BOSS中_解放钢铁旋风(),SlotMode.Always),
        new(new BOSS中_解放原初之魂(),SlotMode.Always),
        new(new BOSS中_混沌旋风(),SlotMode.Always),
        new(new BOSS中_狂魂(),SlotMode.Always),
        new(new BOSS中_保留钢铁旋风(),SlotMode.Always),
        new(new BOSS中_保留原初之魂(),SlotMode.Always),
        new(new BOSS中_钢铁旋风(),SlotMode.Always),
        new(new BOSS中_原初之魂(),SlotMode.Always),
        new(new BOSS中_秘银暴风(),SlotMode.Always),
        new(new BOSS中_超压斧(),SlotMode.Always),
        new(new BOSS中_暴风碎(),SlotMode.Always),
        new(new BOSS中_暴风斩(),SlotMode.Always),
        new(new BOSS中_凶残裂(),SlotMode.Always),
        new(new BOSS中_重劈(),SlotMode.Always),
        new(new BOSS中_飞斧(),SlotMode.Always),
        new(new 拉怪_尽毁(),SlotMode.Always),
        new(new 拉怪_蛮荒崩裂(),SlotMode.Always),
        new(new 拉怪_混沌旋风(),SlotMode.Always),
        new(new 拉怪_钢铁旋风(),SlotMode.Always),
        new(new 拉怪_秘银暴风(),SlotMode.Always),
        new(new 拉怪_超压斧(),SlotMode.Always),
        new(new 拉怪_队友飞斧(),SlotMode.Always),
        new(new 拉怪_无目标飞斧(),SlotMode.Always),
        new(new 拉怪_暴风碎(),SlotMode.Always),
        new(new 拉怪_暴风斩(),SlotMode.Always),
        new(new 拉怪_凶残裂(),SlotMode.Always),
        new(new 拉怪_重劈(),SlotMode.Always),
        new(new 拉怪_飞斧(),SlotMode.Always),
        new(new 小怪中_尽毁(),SlotMode.Always),
        new(new 小怪中_蛮荒崩裂(),SlotMode.Always),
        new(new 小怪中_混沌旋风(),SlotMode.Always),
        new(new 小怪中_狂魂(),SlotMode.Always),
        new(new 小怪中_钢铁旋风(),SlotMode.Always),
        new(new 小怪中_原初之魂(),SlotMode.Always),
        new(new 小怪中_秘银暴风(),SlotMode.Always),
        new(new 小怪中_超压斧(),SlotMode.Always),
        new(new 小怪中_队友飞斧(),SlotMode.Always),
        new(new 小怪中_暴风碎(),SlotMode.Always),
        new(new 小怪中_暴风斩(),SlotMode.Always),
        new(new 小怪中_凶残裂(),SlotMode.Always),
        new(new 小怪中_重劈(),SlotMode.Always),
        new(new 小怪中_飞斧(),SlotMode.Always),
        new(new 小怪结_尽毁(),SlotMode.Always),
        new(new 小怪结_蛮荒崩裂(),SlotMode.Always),
        new(new 小怪结_混沌旋风(),SlotMode.Always),
        new(new 小怪结_狂魂(),SlotMode.Always),
        new(new 小怪结_钢铁旋风(),SlotMode.Always),
        new(new 小怪结_原初之魂(),SlotMode.Always),
        new(new 小怪结_秘银暴风(),SlotMode.Always),
        new(new 小怪结_超压斧(),SlotMode.Always),
        new(new 小怪结_队友飞斧(),SlotMode.Always),
        new(new 小怪结_暴风碎(),SlotMode.Always),
        new(new 小怪结_暴风斩(),SlotMode.Always),
        new(new 小怪结_凶残裂(),SlotMode.Always),
        new(new 小怪结_重劈(),SlotMode.Always),
        new(new 小怪结_飞斧(),SlotMode.Always),

        //以下是前置
        new(new 前置_立刻死斗(),SlotMode.Always),
        new(new 前置_自动死斗(),SlotMode.Always),
        new(new 前置_开盾(),SlotMode.Always),
        new(new 前置_关盾(),SlotMode.Always),
        new(new 前置_1仇(),SlotMode.Always),
        new(new 前置_强制突进(),SlotMode.Always),
        new(new 前置_立刻挑衅(),SlotMode.Always),
        new(new 前置_立刻退避(),SlotMode.Always),
        new(new 前置_立刻铁壁(),SlotMode.Always),
        new(new 前置_立刻战栗(),SlotMode.Always),
        new(new 前置_立刻戮罪(),SlotMode.Always),
        new(new 前置_立刻雪仇(),SlotMode.Always),
        new(new 前置_立刻血气(),SlotMode.Always),
        new(new 前置_立刻勇猛(),SlotMode.Always),
        new(new 前置_立刻勇猛远敏(),SlotMode.Always),
        new(new 前置_立刻勇猛法系(),SlotMode.Always),
        new(new 前置_立刻泰然(),SlotMode.Always),
        new(new 前置_立刻摆脱(),SlotMode.Always),
        new(new 前置_立刻防击退(),SlotMode.Always),
        new(new 前置_立刻爆发药(),SlotMode.Always),
        new(new 前置_立刻突进(),SlotMode.Always),
        new(new 前置_立刻冲刺(),SlotMode.Always),

        //以下是减伤
        new(new 自动泰然(),SlotMode.Always),
        new(new 自动血气_血量(),SlotMode.Always),
        new(new 自动血气_死刑(),SlotMode.Always),
        new(new 自动戮罪_死刑(),SlotMode.Always),
        new(new 自动铁壁_死刑(),SlotMode.Always),
        new(new 自动战栗_死刑(),SlotMode.Always),
        new(new 自动雪仇_死刑(),SlotMode.Always),
        new(new 自动血气_受伤(),SlotMode.Always),
        new(new 自动戮罪_受伤(),SlotMode.Always),
        new(new 自动铁壁_受伤(),SlotMode.Always),
        new(new 自动战栗_受伤(),SlotMode.Always),
        new(new 自动雪仇_受伤(),SlotMode.Always),
        new(new 自动雪仇_aoe(),SlotMode.Always),
        new(new 自动摆脱_aoe(),SlotMode.Always),
        new(new 自动血气_拉怪(),SlotMode.Always),
        new(new 自动戮罪_拉怪(),SlotMode.Always),
        new(new 自动雪仇_拉怪(),SlotMode.Always),
        new(new 自动战栗_拉怪(),SlotMode.Always),
        new(new 自动铁壁_拉怪(),SlotMode.Always),


        new(new 自动挑衅_拉怪(),SlotMode.Always),

        //以下是OGCD
        new(new BOSS战斗中_后续爆发药(),SlotMode.Always),
        new(new BOSS战斗中_首个爆发药(),SlotMode.Always),
        new(new BOSS战斗中_解放(),SlotMode.Always),
        new(new BOSS战斗中_怒震(),SlotMode.Always),
        new(new BOSS战斗中_满层战嚎(),SlotMode.Always),
        new(new BOSS战斗中_70狂暴(),SlotMode.Always),
        new(new BOSS战斗中_50狂暴(),SlotMode.Always),
        new(new BOSS战斗中_35狂暴(),SlotMode.Always),
        new(new BOSS战斗中_满层猛攻(),SlotMode.Always),
        new(new BOSS战斗中_战嚎(),SlotMode.Always),
        new(new BOSS战斗中_群山隆起(),SlotMode.Always),
        new(new BOSS战斗中_动乱(),SlotMode.Always),
        new(new BOSS战斗中_猛攻(),SlotMode.Always),
        new(new 泄资源_爆发药(),SlotMode.Always),
        new(new 泄资源_解放(),SlotMode.Always),
        new(new 泄资源_怒震(),SlotMode.Always),
        new(new 泄资源_狂暴(),SlotMode.Always),
        new(new 泄资源_战嚎(),SlotMode.Always),
        new(new 泄资源_群山隆起(),SlotMode.Always),
        new(new 泄资源_动乱(),SlotMode.Always),
        new(new 泄资源_猛攻(),SlotMode.Always),
        new(new 拉怪_怒震(),SlotMode.Always),
        new(new 拉怪_满层战嚎(),SlotMode.Always),
        new(new 拉怪_自动突进(),SlotMode.Always),
        new(new 小怪中_怒震(),SlotMode.Always),
        new(new 小怪中_满层战嚎(),SlotMode.Always),
        new(new 小怪中_解放(),SlotMode.Always),
        new(new 小怪中_70狂暴(),SlotMode.Always),
        new(new 小怪中_50狂暴(),SlotMode.Always),
        new(new 小怪中_35狂暴(),SlotMode.Always),
        new(new 小怪中_满层猛攻(),SlotMode.Always),
        new(new 小怪中_战嚎(),SlotMode.Always),
        new(new 小怪中_群山隆起(),SlotMode.Always),
        new(new 小怪中_动乱(),SlotMode.Always),
        new(new 小怪结_怒震(),SlotMode.Always),
        new(new 小怪结_满层战嚎(),SlotMode.Always),
        new(new 小怪结_满层猛攻(),SlotMode.Always),
        new(new 小怪结_群山隆起(),SlotMode.Always),
        new(new 小怪结_动乱(),SlotMode.Always),


        new(new 前置_奶人(),SlotMode.Always),
    };
    public IOpener opener = new Frost_WAR_Opener();
    public IOpener m1s = new m1s起手();
    public IOpener m2s = new m2s起手();
    public IOpener m3s = new m3s起手();
    public IOpener m4s = new m4s起手();

    private IOpener? GetOpener(uint level)//设置起手
    {
        if (Core.Resolve<MemApiZoneInfo>().GetCurrTerrId() == 1226)
        {
            return m1s;
        }
        if (Core.Resolve<MemApiZoneInfo>().GetCurrTerrId() == 1228)
        {
            return m2s;
        }
        if (Core.Resolve<MemApiZoneInfo>().GetCurrTerrId() == 1230)
        {
            return m3s;
        }
        if (Core.Resolve<MemApiZoneInfo>().GetCurrTerrId() == 1232)
        {
            return m4s;
        }
        return opener;
    }

    // 设置界面
    public void OnDrawSetting()
    {
        Frost_WAR_SettingUI.Instance.Draw();
    }
    private void IRotationUI()
    {
    }

    // Token: 0x06000011 RID: 17 RVA: 0x0000223B File Offset: 0x0000043B
    public IRotationUI GetRotationUI()
    {
        return Frost_WAR_RotationEntry.JobViewWindow;
    }
    public void Dispose()
    {
        // 释放需要释放的东西 没有就留空
        Frost_WAR_RotationEventHandler.Framework.Update -= Frost_WAR_RotationEventHandler.UpdateACR;
    }
}


