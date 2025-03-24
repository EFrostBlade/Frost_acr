using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.CombatRoutine.Module.Opener;
using AEAssist.CombatRoutine.Module.Target;
using AEAssist.Extension;
using AEAssist.Helper;
using AEAssist.MemoryApi;
using Frost;
using Frost.Common;

namespace test.JOB.Opener;

public class FRU_Opener : IOpener
{

    public Action CompeltedAction { get; set; }

    // 技能id 放在前面定义好
    public const uint 冲刺 = 3u;
    public const uint 先锋剑 = 9u;
    public const uint 暴乱剑 = 15u;
    public const uint 战女神之怒 = 21u;
    public const uint 王权剑 = 3539u;
    public const uint 赎罪剑 = 16460u;
    public const uint 祈告剑 = 36918u;
    public const uint 葬送剑 = 36919u;
    public const uint 圣灵 = 7384u;
    public const uint 战逃反应 = 20u;
    public const uint 沥血剑 = 3538u;
    public const uint 安魂祈祷 = 7383u;
    public const uint 绝对统治 = 36921u;
    public const uint 悔罪 = 16459u;
    public const uint 信念之剑 = 25748u;
    public const uint 真理之剑 = 25749u;
    public const uint 英勇之剑 = 25750u;
    public const uint 荣耀之剑 = 36922u;
    public const uint 深奥之灵 = 29u;
    public const uint 偿赎剑 = 25747u;
    public const uint 厄运流转 = 23u;
    public const uint 调停 = 16461u;
    public const uint 全蚀斩 = 7381u;
    public const uint 日珥斩 = 16457u;
    public const uint 圣环 = 16458u;
    public const uint 盾牌猛击 = 16u;
    public const uint 钢铁信念 = 28u;
    public const uint 解除钢铁信念 = 32065;
    public const uint 投盾 = 24u;
    public const uint 预警 = 17u;
    public const uint 极致防御 = 36920u;
    public const uint 壁垒 = 22u;
    public const uint 盾阵 = 3542u;
    public const uint 圣盾阵 = 25746u;
    public const uint 干预 = 7382u;
    public const uint 保护 = 27u;
    public const uint 神圣领域 = 30u;
    public const uint 圣光幕帘 = 3540u;
    public const uint 深仁厚泽 = 3541u;
    public const uint 武装戍卫 = 7385u;
    public const uint 铁壁 = 7531u;
    public const uint 挑衅 = 7533u;
    public const uint 雪仇 = 7535u;
    public const uint 退避 = 7537u;
    public const uint 插言 = 7538u;
    public const uint 下踢 = 7540u;
    public const uint 亲疏自行 = 7548u;


    public void InitCountDown(CountDownHandler countDownHandler)
    {
        Action action = () =>
        {
            Frost_PLD_RotationEntry.JobViewWindow.SetQt("盾姿", false);
        };
        Action setTarget = () =>
        {
            var target = TargetMgr.Instance.Units.Values.First(u => u.DataId is 17819);
            Core.Resolve<MemApiTarget>().SetTarget(target);
        };
        //倒计时处理
        countDownHandler.AddAction(12000, 冲刺, SpellTargetType.Self);
        countDownHandler.AddAction(12000, setTarget);
        countDownHandler.AddAction(10000, 圣光幕帘, SpellTargetType.Self);
        countDownHandler.AddAction(4000, action);
        countDownHandler.AddAction(4000, 解除钢铁信念, SpellTargetType.Self);
        if (Frost_PLD_RotationEntry.JobViewWindow.GetQt("爆发药"))
        {
            countDownHandler.AddPotionAction(3000);
        }
        countDownHandler.AddAction(1500, 圣灵, SpellTargetType.Target);
        countDownHandler.AddAction(0, 调停, SpellTargetType.Target);
    }
    public List<Action<Slot>> Sequence { get; } = new()
    {
        Step0,
        Step1,
        Step2,
        Step3,
        Step4,
        Step5,
        Step6,
        Step7,
        Step8,
        Step9,
        Step10,
        Step11,
        Step12,
        Step13,
    };
    private static void Step0(Slot slot)
    {
        slot.Add(new Spell(先锋剑, SpellTargetType.Target));
        slot.Add(new Spell(战逃反应, SpellTargetType.Self));
        slot.Add(new Spell(厄运流转, SpellTargetType.Self));
    }
    private static void Step1(Slot slot)
    {
        slot.Add(new Spell(暴乱剑, SpellTargetType.Target));
        slot.Add(new Spell(偿赎剑, SpellTargetType.Target));
    }
    private static void Step2(Slot slot)
    {
        slot.Add(new Spell(王权剑, SpellTargetType.Target));
    }
    private static void Step3(Slot slot)
    {
        slot.Add(new Spell(沥血剑, SpellTargetType.Target));
        slot.Add(new Spell(绝对统治, SpellTargetType.Target));
        slot.Add(new Spell(调停, SpellTargetType.Target));
    }
    private static void Step4(Slot slot)
    {
        slot.Add(new Spell(悔罪, SpellTargetType.Target));
        slot.Add(new Spell(钢铁信念, SpellTargetType.Self));
        Frost_PLD_RotationEntry.JobViewWindow.SetQt("盾姿", true);
        //slot.Add(new Spell(雪仇, SpellTargetType.Self));
    }
    private static void Step5(Slot slot)
    {
        slot.Add(new Spell(信念之剑, SpellTargetType.Target));
    }
    private static void Step6(Slot slot)
    {
        slot.Add(new Spell(真理之剑, SpellTargetType.Target));
    }
    private static void Step7(Slot slot)
    {
        slot.Add(new Spell(英勇之剑, SpellTargetType.Target));
        slot.Add(new Spell(荣耀之剑, SpellTargetType.Target));
        slot.Add(new Spell(挑衅, SpellTargetType.Target));
    }
    private static void Step8(Slot slot)
    {
        slot.Add(new Spell(圣灵, SpellTargetType.Target));
        slot.Add(new Spell(干预, SpellTargetType.Pm2));
    }
    private static void Step9(Slot slot)
    {
        slot.Add(new Spell(赎罪剑, SpellTargetType.Target));
    }
    private static void Step10(Slot slot)
    {
        slot.Add(new Spell(祈告剑, SpellTargetType.Target));
        slot.Add(new Spell(铁壁, SpellTargetType.Self));
    }
    private static void Step11(Slot slot)
    {
        slot.Add(new Spell(圣灵, SpellTargetType.Target));
        slot.Add(new Spell(调停, SpellTargetType.Target));
    }
    private static void Step12(Slot slot)
    {
        slot.Add(new Spell(圣灵, SpellTargetType.Target));
        slot.Add(new Spell(厄运流转, SpellTargetType.Target));
    }
    private static void Step13(Slot slot)
    {
        Frost_PLD_RotationEntry.scWindow.SetSCDuration("极致防御", 5);
        Frost_PLD_RotationEntry.scWindow.SetSC("极致防御", true);
        Frost_PLD_RotationEntry.scWindow.SetSCDuration("壁垒", 5);
        Frost_PLD_RotationEntry.scWindow.SetSC("壁垒", true);
        Frost_PLD_RotationEntry.scWindow.SetSCTarget("干预", TargetType.搭档);
        Frost_PLD_RotationEntry.scWindow.SetSCDuration("干预", 5);
        Frost_PLD_RotationEntry.scWindow.SetSC("干预", true);
        slot.Add(new Spell(葬送剑, SpellTargetType.Target));
        slot.Add(new Spell(偿赎剑, SpellTargetType.Target));
    }
}