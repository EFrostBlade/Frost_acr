using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Extension;
using AEAssist.Helper;
using Dalamud.Game.ClientState.Objects.Types;

namespace Frost.Frost_WAR.Frost_WAR_SlotResolvers
{
    internal class 泄资源_爆发药 : Frost_WAR_ISlotResolver
    {
        public override int Check()
        {

            if (BattleData.ResolverState != 1)
            {
                return -1;
            }
            if (!QT.GetQt("爆发药"))
            {
                return -2;
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

