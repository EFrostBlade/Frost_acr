using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Extension;
using AEAssist.Helper;
using Dalamud.Game.ClientState.Objects.Types;

namespace Frost.Frost_WAR.Frost_WAR_SlotResolvers
{
    internal class 前置_立刻爆发药 : Frost_WAR_ISlotResolver
    {
        public override int Check()
        {
            if (!QT.GetQt("立刻爆发药"))
            {
                return -1;
            }
            double cooldown = SpellsDefine.Potion.GetSpell().Cooldown.TotalSeconds;
            if (cooldown > Setting.cd预检测阈值)
            {
                Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("爆发药CD中,QT已自动关闭");
                QT.SetQt("立刻爆发药", false);
                return -401;
            }
            
            if(cooldown > 0)
            {
                return -400;
            }

            Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog("立刻爆发药");
            spell = Spell.CreatePotion();
            return 0;
        }
    }
}

