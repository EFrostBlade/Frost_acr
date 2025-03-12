using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Extension;
using AEAssist.Helper;
using AEAssist.MemoryApi;
using Dalamud.Game.ClientState.Objects.Types;
using Frost.Frost_WAR.Frost_WAR_Data;

namespace Frost.Frost_WAR.Frost_WAR_SlotResolvers
{
    internal class BOSS战斗中_满层猛攻 : Frost_WAR_ISlotResolver
    {
        public override int Check()
        {
            if(Setting.禁用所有位移技能)
            {
                return -1919;
            }

            if (BattleData.ResolverState != 5)
            {
                return -1;
            }
            if (SpellHelper.GetSpell((uint)WARActionID.猛攻).Charges > 2.85f&&BattleData.猛攻溢出提醒==false)
            {
                Core.Resolve<MemApiChatMessage>().Toast2($"猛攻即将溢出，请靠近boss并停止移动或开启允许突进", 1, 3000);
                ChatHelper.Print.Echo("猛攻即将溢出，请靠近boss并停止移动或开启允许突进<se.1><se.1><se.1>");
                ChatHelper.SendMessage("/pdr tts 猛攻即将溢出");
                BattleData.猛攻溢出提醒 = true;
            }
            if (SpellHelper.GetSpell((uint)WARActionID.猛攻).Charges < 2.9f)
            {
                return -2;
            }
            int baseCheck = CanUseAttactOGCD((uint)WARActionID.猛攻, false, false, 1);
            if (baseCheck != 0)
            {
                return baseCheck;
            }
            if (!QT.GetQt("允许突进"))
            {
                if (BattleData.目标距离 > 0)
                {
                    return -3;
                }
                if (Core.Resolve<MemApiMove>().IsMoving())
                {
                    return -4;
                }
            }


            SpellID = (uint)WARActionID.猛攻;
            target = BattleData.当前目标;
            return base.Check();
        }
    }
}

