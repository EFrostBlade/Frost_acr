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
public class m3s起手 : IOpener
{
    public Action CompeltedAction { get; set; }

    // 技能id 放在前面定义好
    public const uint 铁壁 = 7531u;
    public const uint 战栗 = 40u;
    public const uint 原初的血气 = 25751u;
    public const uint 冲刺 = 3u;
    public const uint 摆脱 = 7388u;
    public const uint 戮罪 = 36923u;
    public const uint 猛攻 = 7386u;
    public const uint 重劈 = 31u;
    public const uint 凶残裂 = 37u;
    public const uint 战嚎 = 52u;
    public const uint 暴风碎 = 45u;
    public const uint 原初的解放 = 7389u;
    public const uint 雪仇 = 7535u;
    public const uint 挑衅 = 7533u;
    public const uint 狂魂 = 16465u;
    public const uint 动乱 = 7387u;


    // 如果需要更多步骤 就不停的添加对应的方法即可
    // 如下，留空代表使用ACR提供的起手
    // public List<Action<Slot>> Sequence { get; } = new();


    // 处理倒计时期间的行为
    public void InitCountDown(CountDownHandler countDownHandler)
    {
        // 倒数14秒的时候跳舞  (如果当前倒计时直接从10秒开始 这个也会触发 多个同时触发的 会按顺序处理)

        // 控制某个ACR的QT 开发工具里要引用对应acr的dll 否则没法让ide检查是否有报错
        //AE.Bard.BardRotationEntry.QT.SetQt(AE.Bard.QTKey.UseBaseGcd, false);

        countDownHandler.AddAction(16000, 战栗);
        countDownHandler.AddAction(15000, 冲刺);
        countDownHandler.AddAction(14000, 原初的血气);
        countDownHandler.AddAction(13000, 摆脱);

        countDownHandler.AddAction(1000, 猛攻, SpellTargetType.Target);
    }


    public List<Action<Slot>> Sequence { get; } = new()
    {
        Step0,
        Step1,
        Step2,
        Step3,
    };


    private static void Step0(Slot slot)
    {
        slot.Add(new Spell(重劈, SpellTargetType.Target));
        slot.Add(new Spell(挑衅, SpellTargetType.Target));
        slot.Add(new Spell(战嚎, SpellTargetType.Self));
    }

    private static void Step1(Slot slot)
    {
        slot.Add(new Spell(凶残裂, SpellTargetType.Target));
        slot.Add(new Spell(雪仇, SpellTargetType.Self));
    }

    private static void Step2(Slot slot)
    {
        slot.Add(new Spell(暴风碎, SpellTargetType.Target));
        if (Frost_WAR_RotationEntry.JobViewWindow.GetQt("爆发药"))
        {
            slot.Add(Spell.CreatePotion());
        }
        slot.Add(new Spell(原初的解放, SpellTargetType.Self));
    }

    private static void Step3(Slot slot)
    {
        slot.Add(new Spell(狂魂, SpellTargetType.Target));
        slot.Add(new Spell(动乱, SpellTargetType.Target));
        slot.Add(new Spell(戮罪, SpellTargetType.Self));
    }
}
