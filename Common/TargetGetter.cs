using AEAssist;
using AEAssist.CombatRoutine.Module.Target;
using AEAssist.Define;
using AEAssist.Extension;
using AEAssist.Helper;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Frost.Frost_PLD.Frost_PLD_Data;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Frost.Common
{
    class TargetGetter
    {
        public static IBattleChara? GetLowestHealthPartyMember(float range = 30)
        {
            IBattleChara? lowestHpTarget = null;
            float lowestHpPercent = 1f;

            foreach (IBattleChara partyer in PartyHelper.CastableAlliesWithin30)
            {
                if (partyer.CurrentHpPercent() <= lowestHpPercent)
                {
                    lowestHpPercent = partyer.CurrentHpPercent();
                    lowestHpTarget = partyer;
                }
            }
            return lowestHpTarget;
        }
        public static IBattleChara? GetTank(float range = 30)
        {
            IBattleChara? tank = null;
            float lowestHpPercent = 1f;
            foreach (IBattleChara partyer in PartyHelper.CastableAlliesWithin30)
            {
                if (partyer.CurrentHpPercent() <= lowestHpPercent && partyer.IsTank())
                {
                    lowestHpPercent = partyer.CurrentHpPercent();
                    tank = partyer;
                }
            }
            return tank;
        }
        public static IBattleChara? GetHealer(float range = 30)
        {
            IBattleChara? healer = null;
            float lowestHpPercent = 1f;
            foreach (IBattleChara partyer in PartyHelper.CastableAlliesWithin30)
            {
                if (partyer.CurrentHpPercent() <= lowestHpPercent && partyer.IsHealer())
                {
                    lowestHpPercent = partyer.CurrentHpPercent();
                    healer = partyer;
                }
            }
            return healer;
        }
        public static IBattleChara? GetMelee(float range = 30)
        {
            IBattleChara? melee = null;
            float lowestHpPercent = 1f;
            foreach (IBattleChara partyer in PartyHelper.CastableAlliesWithin30)
            {
                if (partyer.CurrentHpPercent() <= lowestHpPercent && partyer.IsMelee())
                {
                    lowestHpPercent = partyer.CurrentHpPercent();
                    melee = partyer;
                }
            }
            return melee;
        }
        public static IBattleChara? GetRanged(float range = 30)
        {
            IBattleChara? ranged = null;
            float lowestHpPercent = 1f;
            foreach (IBattleChara partyer in PartyHelper.CastableAlliesWithin30)
            {
                if (partyer.CurrentHpPercent() <= lowestHpPercent && partyer.IsRanged())
                {
                    lowestHpPercent = partyer.CurrentHpPercent();
                    ranged = partyer;
                }
            }
            return ranged;
        }
        public static IBattleChara? GetCaster(float range = 30)
        {
            IBattleChara? caster = null;
            float lowestHpPercent = 1f;
            foreach (IBattleChara partyer in PartyHelper.CastableAlliesWithin30)
            {
                if (partyer.CurrentHpPercent() <= lowestHpPercent && partyer.IsCaster())
                {
                    lowestHpPercent = partyer.CurrentHpPercent();
                    caster = partyer;
                }
            }
            return caster;
        }
        public static unsafe IBattleChara? GetPartyMember(int no)
        {
            AgentHUD* agentHud = AgentHUD.Instance();
            if (agentHud == null)
            {
                //LogHelper.PrintError("AgentHUD不可用。");
                return null;
            }

            int partyMemberCount = agentHud->PartyMemberCount;

            if (partyMemberCount < 0 || partyMemberCount > 10)
            {
                //LogHelper.PrintError($"无效的partyMemberCount：{partyMemberCount}");
                return null;
            }

            if (Core.Me.HasCanDispel())
                return Core.Me;


            for (int i = 0; i < partyMemberCount; i++)
            {
                HudPartyMember member;
                fixed (HudPartyMember* memberPtr = &agentHud->PartyMembers[0])
                {
                    member = memberPtr[i];
                }

                var obj = Svc.Objects.SearchByEntityId(member.EntityId);

                if (obj is IBattleChara BattleChara && i == no)
                {
                    return BattleChara;
                }
            }

            return null;
        }

        public static IBattleChara? GetBattleCharaByName(string name)
        {

            Dictionary<uint, IBattleChara> enemys = TargetMgr.Instance.Enemys;
            if (enemys.Count == 0) return null;


            foreach (var kvp in enemys)
            {
                var enemy = kvp.Value;
                if (!enemy.ValidAttackUnit()) continue;
                if (enemy.Name.ToString() == name)
                {
                    return enemy;
                }
            }
            return null;
        }

        public static IBattleChara? GetBattleCharaByDataID(uint dataID)
        {
            Dictionary<uint, IBattleChara> enemys = TargetMgr.Instance.Enemys;
            if (enemys.Count == 0) return null;


            foreach (var kvp in enemys)
            {
                var enemy = kvp.Value;
                if (!enemy.ValidAttackUnit()) continue;
                if (enemy.DataId == dataID)
                {
                    return enemy;
                }
            }
            return null;
        }
    }
}
