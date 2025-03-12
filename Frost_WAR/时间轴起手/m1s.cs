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

// �������߽���:
// ֻ��Ϊ��ѧϰд�ű���֮ǰû�Ӵ������ ����û�Ӵ���C#�� ��ʹ��vs code ���ٶ�[�����visual studio code�ﰲװ.net��������]
// �������ѧϰ��ʹ��C#����������Ŀ��� ���鰲װvisual studio / rider�����°� ����װ.net����
// �����ű�ʱ ������AEAssist+Dalamud+������Ҫ��dll
public class m1s���� : IOpener
{
    public Action CompeltedAction { get; set; }

    // ����id ����ǰ�涨���
    public const uint ���� = 7531u;
    public const uint ս�� = 40u;
    public const uint ԭ����Ѫ�� = 25751u;
    public const uint ��� = 3u;
    public const uint ���� = 7388u;
    public const uint ¾�� = 36923u;
    public const uint �͹� = 7386u;
    public const uint ���� = 31u;
    public const uint �ײ��� = 37u;
    public const uint ս�� = 52u;
    public const uint ������ = 45u;
    public const uint ԭ���Ľ�� = 7389u;
    public const uint ѩ�� = 7535u;
    public const uint ���� = 7533u;


    // �����Ҫ���ಽ�� �Ͳ�ͣ����Ӷ�Ӧ�ķ�������
    // ���£����մ���ʹ��ACR�ṩ������
    // public List<Action<Slot>> Sequence { get; } = new();


    // ������ʱ�ڼ����Ϊ
    public void InitCountDown(CountDownHandler countDownHandler)
    {
        // ����14���ʱ������  (�����ǰ����ʱֱ�Ӵ�10�뿪ʼ ���Ҳ�ᴥ�� ���ͬʱ������ �ᰴ˳����)
        // countDownHandler.AddAction(20000, ����);

        // ����ĳ��ACR��QT ����������Ҫ���ö�Ӧacr��dll ����û����ide����Ƿ��б���
        //AE.Bard.BardRotationEntry.QT.SetQt(AE.Bard.QTKey.UseBaseGcd, false);

        countDownHandler.AddAction(15000, ս��);
        countDownHandler.AddAction(14000, ԭ����Ѫ��);
        countDownHandler.AddAction(13000, ���);
        countDownHandler.AddAction(10000, ����);

        countDownHandler.AddAction(2000, ¾��);
        countDownHandler.AddAction(1000, �͹�, SpellTargetType.Target);
    }


    public List<Action<Slot>> Sequence { get; } = new()
    {
        Step0,
        Step1,
        Step2,
    };


    private static void Step0(Slot slot)
    {
        slot.Add(new Spell(����, SpellTargetType.Target));
        slot.Add(new Spell(����, SpellTargetType.Target));
        slot.Add(new Spell(ս��, SpellTargetType.Self));
    }

    private static void Step1(Slot slot)
    {
        slot.Add(new Spell(�ײ���, SpellTargetType.Target));
        slot.Add(new Spell(ѩ��, SpellTargetType.Self));
    }

    private static void Step2(Slot slot)
    {
        slot.Add(new Spell(������, SpellTargetType.Target));
        if (Frost_WAR_RotationEntry.JobViewWindow.GetQt("����ҩ"))
        {
            slot.Add(Spell.CreatePotion());
        }
        slot.Add(new Spell(ԭ���Ľ��, SpellTargetType.Self));
    }

}
