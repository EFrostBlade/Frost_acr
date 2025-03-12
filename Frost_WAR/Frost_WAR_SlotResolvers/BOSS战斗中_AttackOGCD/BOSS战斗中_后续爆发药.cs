using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Extension;
using AEAssist.Helper;
using Dalamud.Game.ClientState.Objects.Types;
using Frost.Frost_WAR.Frost_WAR_Data;

namespace Frost.Frost_WAR.Frost_WAR_SlotResolvers
{
    internal class BOSS战斗中_后续爆发药 : Frost_WAR_ISlotResolver
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
            if (BattleData.战斗开始分钟数 < 1)
            {
                return -3;
            }

            double cooldown = SpellsDefine.Potion.GetSpell().Cooldown.TotalSeconds;

            if (cooldown > 0)
            {
                return -400;
            }
            if (BattleData.战场风暴剩余时间 == 0f)
            {
                return -4;
            }
            if (BattleData.尽毁预备剩余时间 == 0f)
            {
                return -5;
            }
            if (BattleData.尽毁预备剩余时间 > 3f)
            {
                return -6;
            }
            if ((float)SpellHelper.GetSpell((uint)WARActionID.原初的解放).Cooldown.TotalSeconds > 13f)
            {
                return -7;
            }

            spell = Spell.CreatePotion();
            return 0;
        }
    }
}

