using AEAssist.CombatRoutine;
using Dalamud.Game.ClientState.Objects.Types;

namespace Frost.Frost_WAR.Frost_WAR_Data
{

    public enum 战斗状态
    {
        无战斗,
        野外小怪战斗中,
        野外小怪战斗即将结束,
        野外BOSS战斗中,
        野外BOSS战斗即将结束,
        四人本拉怪途中,
        四人本小怪战斗中,
        四人本小怪战斗即将结束,
        四人本BOSS战斗中,
        四人本BOSS战斗即将结束,
        八人本小怪战斗中,
        八人本小怪战斗即将结束,
        八人本BOSS战斗中,
        八人本BOSS战斗即将结束,
        团本小怪战斗中,
        团本小怪战斗即将结束,
        团本BOSS战斗中,
        团本BOSS战斗即将结束,
        高难本BOSS战斗中,
        高难本BOSS战斗即将结束,
    }


    internal class Frost_WAR_BattleData
    {
        public static Frost_WAR_BattleData Instance = new();
        public Spell? nextOGCD = null;
        public Spell? nextGCD = null;
        public uint 上次连击技能 = 0;
        public ulong 上次挑衅目标 = 0;
        public ulong 上次飞斧目标 = 0;
        public bool 当前GCD用过减伤 = false;
        public bool 蛮荒过期提醒 = false;
        public bool 猛攻溢出提醒 = false;
        public bool 目标是否在近战范围内 = false;
        public bool 是否可打aoe = false;
        public bool 已开保留尽毁 = false;
        public bool 已开泄资源 = false;
        public bool 是否开盾 = false;
        public bool 是否战技封印 = false;
        public bool 是否魔法封印 = false;
        public bool 是否能力封印 = false;
        public IBattleChara? 当前目标 = null;
        public float 目标距离 = 0;
        public int 以自身为目标的敌人数量 = 0;
        public List<IBattleChara> 无目标的敌人列表 = new();
        public List<IBattleChara> 以自身为目标的敌人列表 = new();
        public List<IBattleChara> 以队友为目标的敌人列表 = new();
        public List<IBattleChara> 以其他为目标的敌人列表 = new();
        public bool isChecking = false;
        public 战斗状态 当前战斗状态 = 战斗状态.无战斗;
        public 战斗状态 上次战斗状态 = 战斗状态.无战斗;
        /// <summary>
        ///-1:停手 0:无战斗 1:泄资源 2:拉怪 3:小怪战斗 4:小怪即将结束 5:BOSS战斗 
        /// </summary>
        public int ResolverState = 0;
        public float 战场风暴剩余时间 = 0;
        public float 原初的解放层数 = 0;
        public float 原初的觉悟剩余时间 = 0;
        public float 原初的混沌剩余时间 = 0;
        public float 爆发药剩余时间 = 0;
        public float 蛮荒崩裂预备剩余时间 = 0;
        public float 尽毁预备剩余时间 = 0;
        public float 原初的怒震预备剩余时间 = 0;
        public float 死斗剩余时间 = 0;
        public float 狂暴剩余时间 = 0;
        public int 自身减伤比例 = 0;
        public int 队伍减伤比例 = 0;
        public int 自身护盾 = 0;
        public int 队伍护盾 = 0;
        public string 持有单减 = "";
        public string 持有目标减 = "";
        public string 持有群减 = "";
        public string 持有单盾 = "";
        public string 持有群盾 = "";
        public string buff列表 = "";
        public int 已打解放次数 = 0;
        public long 战斗开始时长 = 0;
        public uint 战斗开始分钟数 = 0;
        public LinkedList<(uint, long)> 自身血量记录 = new LinkedList<(uint, long)>();
        public Dictionary<int, int> 自身每秒承伤 = new Dictionary<int, int>();
    }
}
