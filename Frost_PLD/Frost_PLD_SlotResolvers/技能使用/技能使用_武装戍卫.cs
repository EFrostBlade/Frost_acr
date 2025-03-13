using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.CombatRoutine.Module.Target;
using AEAssist.CombatRoutine.Trigger.TriggerAction;
using AEAssist.Define;
using AEAssist.Extension;
using AEAssist.Helper;
using AEAssist.MemoryApi;
using Dalamud.Game.ClientState.Objects.Types;
using Frost.Frost_PLD.Frost_PLD_Data;
using Frost.Frost_WAR.Frost_WAR_Data;
using System.Numerics;

namespace Frost.Frost_PLD.Frost_PLD_SlotResolvers
{
    internal class 技能使用_武装戍卫 : Frost_PLD_ISlotResolver
    {
        public override int Check()
        {
            if (!SC.GetSC("武装戍卫"))
            {
                return -1;
            }

            int baseCheck = CanUseDefenceOGCD((uint)PLDActionID.武装戍卫, false, SC.GetSCForceInsert("武装戍卫"));
            if (baseCheck == -600)
            {
                return -600;
            }
            if (baseCheck != 0)
            {
                return baseCheck;
            }
            double cooldown = SpellHelper.GetSpell((uint)PLDActionID.武装戍卫).Cooldown.TotalSeconds;
            if (cooldown > Setting.技能提前时间)
            {
                return -400;
            }

            Frost_PLD_RotationEntry.Frost_PLD_ArcUI.AddLog("武装戍卫 技能使用");
            SpellID = (uint)PLDActionID.武装戍卫;
            target = SC.GetSCTarget("武装戍卫");
            if (target == null)
            {
                return -404;
            }


            // 通过 Core.Me 获取自身位置，其中 Position 为 Vector3 类型
            var self = Core.Me;
            Vector3 selfPos = self.Position;
            var allies = PartyHelper.CastableAlliesWithin10;
            int bestCount = 0;
            float bestAngle = 0;
            List<IBattleChara> bestEffectAlly = new List<IBattleChara>();

            // 遍历候选角度 0 ～ 359 度
            for (int candidateDeg = 0; candidateDeg < 360; candidateDeg++)
            {
                float rad = candidateDeg * MathF.PI / 180;
                // 构造候选方向向量（忽略 Y 轴高度，只考虑 XZ 平面）
                Vector3 candidateDir = new Vector3(MathF.Cos(rad), 0, MathF.Sin(rad));
                int count = 0;
                List<IBattleChara> effectAlly = new List<IBattleChara>();
                foreach (var ally in allies)
                {
                    // 计算从自身到队友的向量
                    Vector3 diff = ally.Position - selfPos;
                    if (diff.Length() > 8f)
                        continue;
                    diff = Vector3.Normalize(diff);
                    // 计算候选方向和队友方向的夹角（单位：度）
                    float dot = Vector3.Dot(candidateDir, diff);
                    float angleDeg = MathF.Acos(dot) * 180 / MathF.PI;
                    // 检测队友是否落在候选扇形内（左右各 60°）
                    if (angleDeg <= 60)
                    {
                        count++;
                        effectAlly.Add(ally);
                    }

                }
                if (count > bestCount)
                {
                    bestCount = count;
                    bestAngle = candidateDeg;
                    bestEffectAlly = effectAlly;
                }
            }
            string log = $"最佳释放角度{bestAngle} 队友数量{bestCount}";
            if (bestEffectAlly.Count > 0)
            {
                foreach (var ally in bestEffectAlly)
                {
                    log += $" {ally.Name}";
                }
            }
            Frost_PLD_RotationEntry.Frost_PLD_ArcUI.AddLog(log);


            // 将最佳面向转换为 SetRot 所需要的弧度值（0代表正南，π/2代表正东，-π/2代表正西）
            float candidateRad = bestAngle * MathF.PI / 180f;
            float setRot = candidateRad - 3f * MathF.PI / 2f;
            // 归一化到 [-π, π]
            while (setRot > MathF.PI)
                setRot -= 2f * MathF.PI;
            while (setRot < -MathF.PI)
                setRot += 2f * MathF.PI;
            Core.Resolve<MemApiMove>().SetRot(setRot);

            return base.Check();
        }
    }
}