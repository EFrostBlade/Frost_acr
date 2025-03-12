using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Extension;
using AEAssist.Helper;
using Dalamud.Game.ClientState.Objects.Types;

namespace Frost.Frost_WAR.Frost_WAR_SlotResolvers
{
    internal class BOSS战斗中_首个爆发药 : Frost_WAR_ISlotResolver
    {
        public override int Check()
        {

            if (BattleData.ResolverState != 5)
            {
                return -1;
            }
            if (!QT.GetQt("爆发药"))
            {
                return -2;
            }
            if (BattleData.战斗开始分钟数 > 1)
            {
                return -3;
            }
            if (BattleData.战场风暴剩余时间 == 0f)
            {
                return -4;
            }


            double cooldown = SpellsDefine.Potion.GetSpell().Cooldown.TotalSeconds;

            if (cooldown > 0)
            {
                return -400;
            }

            spell = Spell.CreatePotion();
            return 0;
        }
    }
}

