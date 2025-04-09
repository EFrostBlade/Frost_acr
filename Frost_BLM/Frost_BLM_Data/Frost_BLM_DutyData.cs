using AEAssist.CombatRoutine;
using Dalamud.Game.ClientState.Objects.Types;

namespace Frost.Frost_BLM.Frost_BLM_Data
{
    internal class Frost_BLM_DutyData
    {
        public static Frost_BLM_DutyData Instance = new();
        public bool 是否在副本中 = false;
        public bool 是否高难 = false;
        public int 副本人数 = 0;
        public DateTime 上次疾跑时间 = DateTime.MinValue;
        public List<IBattleChara> 无法拉仇恨的敌人列表 = new();
    }
}
