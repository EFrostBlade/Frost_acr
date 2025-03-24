using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.CombatRoutine.Module.Opener;
using AEAssist.Helper;
using AEAssist.JobApi;
using AEAssist.MemoryApi;
using Frost;
using System.Numerics;

namespace ScriptTest;

// 开发工具建议:
// 只是为了学习写脚本，之前没接触过编程 或者没接触过C#的 请使用vs code 并百度[如何在visual studio code里安装.net开发环境]
// 如果有意学习且使用C#语言做更深的开发 建议安装visual studio / rider的最新版 并安装.net环境
// 开发脚本时 请引用AEAssist+Dalamud+其他必要的dll
public class M1s_Opener : IOpener
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



    // 如果需要更多步骤 就不停的添加对应的方法即可
    // 如下，留空代表使用ACR提供的起手
    // public List<Action<Slot>> Sequence { get; } = new();


    // 处理倒计时期间的行为
    public void InitCountDown(CountDownHandler countDownHandler)
    {
        // 倒数14秒的时候跳舞  (如果当前倒计时直接从10秒开始 这个也会触发 多个同时触发的 会按顺序处理)
        // countDownHandler.AddAction(20000, 铁壁);

        // 控制某个ACR的QT 开发工具里要引用对应acr的dll 否则没法让ide检查是否有报错
        //AE.Bard.BardRotationEntry.QT.SetQt(AE.Bard.QTKey.UseBaseGcd, false);

        countDownHandler.AddAction(10000, 圣光幕帘);
        countDownHandler.AddAction(1800, 圣灵, SpellTargetType.Target);
        countDownHandler.AddAction(300, 调停, SpellTargetType.Target);
    }


    public List<Action<Slot>> Sequence { get; } = new()
    {
        Step0,
        Step1,
        Step2,
    };


    private static void Step0(Slot slot)
    {
        slot.Add(new Spell(先锋剑, SpellTargetType.Target));
    }

    private static void Step1(Slot slot)
    {
        slot.Add(new Spell(暴乱剑, SpellTargetType.Target));
    }

    private static void Step2(Slot slot)
    {
        slot.Add(new Spell(王权剑, SpellTargetType.Target));
        slot.Add(new Spell(战逃反应, SpellTargetType.Self));
        slot.Add(new Spell(绝对统治, SpellTargetType.Target));
    }

}
