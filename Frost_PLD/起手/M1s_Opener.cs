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
public class M1s_Opener : IOpener
{
    public Action CompeltedAction { get; set; }

    // ����id ����ǰ�涨���
    public const uint ��� = 3u;
    public const uint �ȷ潣 = 9u;
    public const uint ���ҽ� = 15u;
    public const uint սŮ��֮ŭ = 21u;
    public const uint ��Ȩ�� = 3539u;
    public const uint ���｣ = 16460u;
    public const uint ��潣 = 36918u;
    public const uint ���ͽ� = 36919u;
    public const uint ʥ�� = 7384u;
    public const uint ս�ӷ�Ӧ = 20u;
    public const uint ��Ѫ�� = 3538u;
    public const uint ������ = 7383u;
    public const uint ����ͳ�� = 36921u;
    public const uint ���� = 16459u;
    public const uint ����֮�� = 25748u;
    public const uint ����֮�� = 25749u;
    public const uint Ӣ��֮�� = 25750u;
    public const uint ��ҫ֮�� = 36922u;
    public const uint ���֮�� = 29u;
    public const uint ���꽣 = 25747u;
    public const uint ������ת = 23u;
    public const uint ��ͣ = 16461u;
    public const uint ȫʴն = 7381u;
    public const uint ����ն = 16457u;
    public const uint ʥ�� = 16458u;
    public const uint �����ͻ� = 16u;
    public const uint �������� = 28u;
    public const uint Ͷ�� = 24u;
    public const uint Ԥ�� = 17u;
    public const uint ���·��� = 36920u;
    public const uint ���� = 22u;
    public const uint ���� = 3542u;
    public const uint ʥ���� = 25746u;
    public const uint ��Ԥ = 7382u;
    public const uint ���� = 27u;
    public const uint ��ʥ���� = 30u;
    public const uint ʥ��Ļ�� = 3540u;
    public const uint ���ʺ��� = 3541u;
    public const uint ��װ���� = 7385u;
    public const uint ���� = 7531u;
    public const uint ���� = 7533u;
    public const uint ѩ�� = 7535u;
    public const uint �˱� = 7537u;
    public const uint ���� = 7538u;
    public const uint ���� = 7540u;
    public const uint �������� = 7548u;



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

        countDownHandler.AddAction(10000, ʥ��Ļ��);
        countDownHandler.AddAction(1800, ʥ��, SpellTargetType.Target);
        countDownHandler.AddAction(300, ��ͣ, SpellTargetType.Target);
    }


    public List<Action<Slot>> Sequence { get; } = new()
    {
        Step0,
        Step1,
        Step2,
    };


    private static void Step0(Slot slot)
    {
        slot.Add(new Spell(�ȷ潣, SpellTargetType.Target));
    }

    private static void Step1(Slot slot)
    {
        slot.Add(new Spell(���ҽ�, SpellTargetType.Target));
    }

    private static void Step2(Slot slot)
    {
        slot.Add(new Spell(��Ȩ��, SpellTargetType.Target));
        slot.Add(new Spell(ս�ӷ�Ӧ, SpellTargetType.Self));
        slot.Add(new Spell(����ͳ��, SpellTargetType.Target));
    }

}
