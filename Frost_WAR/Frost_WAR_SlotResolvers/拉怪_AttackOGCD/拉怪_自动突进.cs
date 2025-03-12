using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.CombatRoutine.Module.Target;
using AEAssist.Define;
using AEAssist.Extension;
using AEAssist.Helper;
using AEAssist.MemoryApi;
using Dalamud.Game.ClientState.Objects.Types;
using Frost.Frost_WAR.Frost_WAR_Data;
using Frost.Frost_WAR.Frost_WAR_Setting;
using ImGuiNET;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Frost.Frost_WAR.Frost_WAR_SlotResolvers
{
    internal class 拉怪_自动突进 : Frost_WAR_ISlotResolver
    {
        public override int Check()
        {
            if (Setting.禁用所有位移技能)
            {
                return -1919;
            }
            if (BattleData.ResolverState != 2)
            {
                return -1;
            }
            int baseCheck = CanUseAttactOGCD((uint)WARActionID.猛攻, false);
            if (baseCheck != 0)
            {
                return baseCheck;
            }
            if (SpellHelper.GetSpell((uint)WARActionID.猛攻).Charges < 1f)
            {
                return -400;
            }
            if (!Setting.自动拉怪突进)
            {
                return -3;
            }
            if (!Core.Resolve<MemApiMove>().IsMoving())
            {
                return -4;
            }
            foreach (var keyValuePair in TargetMgr.Instance.EnemysIn20)
            {
                IBattleChara enemy = keyValuePair.Value;
                if (GameObjectExtension.Distance(Core.Me, enemy, DistanceMode.IgnoreAll) > 10
                    && Core.Resolve<MemApiSpell>().CheckActionInRangeOrLoS((uint)WARActionID.猛攻, enemy))
                {
                    // 计算目标是否在当前面向范围内
                    var directionToTarget = enemy.Position - Core.Me.Position;
                    var forwardDirection = new Vector3((float)Math.Sin(Core.Me.Rotation), 0, (float)Math.Cos(Core.Me.Rotation));
                    var angle = Math.Acos(Vector3.Dot(Vector3.Normalize(forwardDirection), Vector3.Normalize(directionToTarget))) * (180.0 / Math.PI);

                    if (angle <= 40)
                    {
                        SpellID = (uint)WARActionID.猛攻;
                        target = enemy;
                        Frost_WAR_RotationEntry.Frost_WAR_ArcUI.AddLog($"自动突进{enemy.Name}拉怪");
                        Core.Resolve<MemApiChatMessage>().Toast2($"自动突进{enemy.Name}拉怪", 1, 2000);
                        return base.Check();
                    }
                }
            }


            return -404;
        }
    }
}

