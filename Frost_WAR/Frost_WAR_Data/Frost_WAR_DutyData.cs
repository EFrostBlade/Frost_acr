using AEAssist.CombatRoutine;
using Dalamud.Game.ClientState.Objects.Types;

namespace Frost.Frost_WAR.Frost_WAR_Data
{
    internal class Frost_WAR_DutyData
    {
        public static Frost_WAR_DutyData Instance = new();
        public bool 是否在副本中 = false;
        public bool 是否高难 = false;
        public int 副本人数 = 0;
        public DateTime 上次疾跑时间 = DateTime.MinValue;
        public List<IBattleChara> 无法拉仇恨的敌人列表 = new();
    }
}
