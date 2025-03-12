using AEAssist.CombatRoutine.Module;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Trigger;
using AEAssist.GUI;
using AEAssist.Helper;
using ImGuiNET;
using AEAssist.MemoryApi;
using AEAssist;
using System.Numerics;
using AEAssist.Avoid;
using AEAssist.CombatRoutine.Module.Target;
using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using AEAssist.Extension;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Windows.ApplicationModel.Store.Preview.InstallControl;

namespace Frost.Frost_WAR.Frost_WAR_Triggers
{
    public class 跟随目标 : ITriggerCond
    {

        public string DisplayName { get; } = "通用/跟随目标";
        public string Remark { get; set; }


        public bool Draw()
        {
            ImGui.Text("警告：跟随目标会导致本序列后方的所有检测失效，请单独使用并行节点进行跟随目标");
            ImGui.Text("警告：使用跟随目标后务必在合适的时候使用停止跟随，否则后果自负");

            ImGui.RadioButton("敌人", ref isEnemy, 1);
            ImGui.SameLine();
            ImGui.RadioButton("队友", ref isEnemy, 0);

            if (isEnemy == 1)
            {
                ImGui.InputInt("DataID", ref enemyDataId);
            }
            else
            {
                ImGui.Combo("选择队友", ref selectedTeammate, teammates, teammates.Length);
            }

            return false;
        }

        public int isEnemy = 1; // 1 for enemy, 0 for teammate
        public int enemyDataId = 0;
        public int selectedTeammate = 0;
        public string[] teammates = { "队友1", "队友2", "队友3", "队友4", "队友5", "队友6", "队友7" };

        public static unsafe IBattleChara FindDispelTarget(int no )
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

                if (obj is IBattleChara BattleChara && i==no)
                {
                    return BattleChara;
                }
            }

            return null;
        }

        public bool Handle(ITriggerCondParams triggerCondParams)
        {
            if (!Frost_WAR_RotationEntry.时间轴自动跟随启用情况 && !Frost_WAR_RotationEntry.时间轴自动跟随启用启用情况)
            {
                Frost_WAR_RotationEntry.时间轴自动跟随启用情况 = true;
                Frost_WAR_RotationEntry.时间轴自动跟随启用启用情况 = true;
            }
            else if (!Frost_WAR_RotationEntry.时间轴自动跟随启用情况 && Frost_WAR_RotationEntry.时间轴自动跟随启用启用情况)
            {
                Frost_WAR_RotationEntry.时间轴自动跟随启用情况 = false;
                Frost_WAR_RotationEntry.时间轴自动跟随启用启用情况 = false;
                return true;
            }
            if (isEnemy == 1)
            {
                foreach (var keyValuePair in TargetMgr.Instance.EnemysIn25)
                {
                    IBattleChara? battleChara = keyValuePair.Value as IBattleChara;
                    if (battleChara != null)
                    {
                        if (battleChara.DataId == enemyDataId)
                        {
                            Core.Resolve<MemApiMove>().MoveToTarget(battleChara.Position);
                        }
                    }
                }
            }
            else
            {
                try
                {
                    IBattleChara battleChara = FindDispelTarget(selectedTeammate+1);
                    Core.Resolve<MemApiMove>().MoveToTarget(battleChara.Position);
                }
                catch (System.Exception e)
                {
                    return false;
                }
            }
            return false;
        }

    }
}
