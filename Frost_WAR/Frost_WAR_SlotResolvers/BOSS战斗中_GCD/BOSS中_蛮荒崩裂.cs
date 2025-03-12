using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.CombatRoutine.Module.Target;
using AEAssist.Extension;
using AEAssist.Helper;
using AEAssist.MemoryApi;
using Dalamud.Game.ClientState.Objects.Types;
using Frost.Frost_WAR.Frost_WAR_Data;
using Frost.HOOK;


namespace Frost.Frost_WAR.Frost_WAR_SlotResolvers
{
    internal class BOSS中_蛮荒崩裂 : Frost_WAR_ISlotResolver
    {
        public override int Check()
        {
            if (Setting.禁用所有位移技能)
            {
                return -1919;
            }

            if (BattleData.ResolverState != 5)
            {
                return -1;
            }
            if (QT.GetQt("保留尽毁"))
            {
                return -2;
            }
            if (BattleData.蛮荒崩裂预备剩余时间 > 0f && BattleData.蛮荒崩裂预备剩余时间 < 5f)
            {
                if (BattleData.蛮荒过期提醒 == false)
                {
                    Core.Resolve<MemApiChatMessage>().Toast2($"蛮荒即将过期，请靠近boss并停止移动或开启允许突进", 2, 3000);
                    ChatHelper.Print.Echo("蛮荒即将过期，请靠近boss并停止移动或开启允许突进<se.1><se.1><se.1>");
                    ChatHelper.SendMessage("/pdr tts 靠近boss停止移动");
                    BattleData.蛮荒过期提醒 = true;
                }
            }

            int baseCheck = CanUseGCD((uint)WARActionID.蛮荒崩裂, 1, false);
            if (baseCheck != 0)
            {
                return baseCheck;
            }
            if (!QT.GetQt("允许突进") && (!Setting.自动无位移蛮荒 || BattleData.蛮荒崩裂预备剩余时间 > 4f))
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

            if (BattleData.蛮荒崩裂预备剩余时间 >4f)
            {
                if (!QT.GetQt("前置尽毁") && BattleData.原初的解放层数 > 0)
                {
                    return -5;
                }
            }
            if (Setting.自动无位移蛮荒)
            {
                if (BattleData.目标距离 > 0 || Core.Resolve<MemApiMove>().IsMoving())
                {
                    Hook.EnablePatch(PatchType.NoActionMove);
                    LogHelper.Print("即将自动无位移蛮荒");
                }
            }
            SpellID = (uint)WARActionID.蛮荒崩裂;
            target = BattleData.当前目标;
            return base.Check();
        }
    }
}

