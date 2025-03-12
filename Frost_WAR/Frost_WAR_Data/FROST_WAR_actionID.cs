using AEAssist;
using AEAssist.CombatRoutine.Module;
using AEAssist.Helper;
using AEAssist.MemoryApi;
using Dalamud.Hooking;

namespace Frost.Frost_WAR.Frost_WAR_Data
{

    public static class SpellUtils
    {
        public static bool CanUseOGCD_NoCombat(uint spellId)
        {
            var battleData = Frost_WAR_BattleData.Instance;
            var QT = Frost_WAR_RotationEntry.JobViewWindow;

            if (Core.Resolve<MemApiSpell>().GetActionState(spellId) == 572)  //释放条件未满足
            {
                return false;
            }
            if (Core.Resolve<MemApiSpell>().GetActionState(spellId) == 573) //尚未学习技能
            {
                return false;
            }

            if (Core.Resolve<MemApiSpell>().GetActionState(spellId) == 579) //状态限制
            {
                return false;
            }


            if (AI.Instance.BattleData.LockSpells.Contains(spellId))
            {
                return false;
            }
            if (battleData.是否能力封印)
            {
                return false;
            }
            if (!Core.Resolve<MemApiSpell>().IsLevelEnough(spellId))
            {
                return false;
            }
            if (spellId.GetSpell().Charges < 1f)
            {
                return false;
            }
            return true;
        }
    }
    public enum WARActionID : uint
    {
        冲刺 = 3u,
        /// <summary>
        ///1级，连击1
        /// </summary>
        重劈 = 31u,
        /// <summary>
        ///4级，连击2，35级获得兽魂10，
        /// </summary>
        凶残裂 = 37u,
        /// <summary>
        ///6级，对自身附加3档的狂暴状态　持续时间：15秒
        ///狂暴效果：一定时间内，自身发动的战技必定暴击并且直击
        ///受暴击或直击发动率提高效果影响时，攻击造成的伤害提高
        ///追加效果：战场风暴的持续时间延长10秒
        ///70级升级为原初的解放
        /// </summary>
        狂暴 = 38u,
        /// <summary>
        ///30级，减伤20%，持续10s，冷却90s，78级提高20%受治疗
        /// </summary>
        战栗 = 40u,
        /// <summary>
        ///10级，aoe连击1
        /// </summary>
        超压斧 = 41u,
        /// <summary>
        ///26级，连击3，回血，35级获得兽魂20
        /// </summary>
        暴风斩 = 42u,
        /// <summary>
        ///42级，无敌，持续10s，冷却240s
        /// </summary>
        死斗 = 43u,
        /// <summary>
        ///38级，减伤30%，反伤，持续15s，冷却120s，92级升级为戮罪
        /// </summary>
        复仇 = 44u,
        /// <summary>
        ///50级，连击3，获得10兽魂
        ///伤害提高10%，持续30s，最高60s
        /// </summary>
        暴风碎 = 45u,
        /// <summary>
        ///15级，远程
        /// </summary>
        飞斧 = 46u,
        /// <summary>
        ///10级，盾姿
        /// </summary>
        守护 = 48u,
        解除守护 = 32066u,
        /// <summary>
        ///35级，消耗50兽魂单体攻击，54级变为裂石飞环
        /// </summary>
        原初之魂 = 49u,
        /// <summary>
        ///45级，消耗50兽魂aoe，60级变为地毁人亡
        /// </summary>
        钢铁旋风 = 51u,
        /// <summary>
        ///50级，获得50点兽魂
        ///可储存2层，冷却60s
        ///66级每使用一次兽魂技加快5s
        ///72级强化下一次地毁人亡为混沌旋风，80级强化下一次裂石飞环为狂魂
        /// </summary>
        战嚎 = 52u,
        /// <summary>
        ///54级，消耗50兽魂单体攻击
        /// </summary>
        裂石飞环 = 3549u,
        /// <summary>
        ///60级，消耗50兽魂aoe
        /// </summary>
        地毁人亡 = 3550u,
        /// <summary>
        ///56级，减伤10%，吸血，持续6s，冷却25s，82级升级为原初的血气
        /// </summary>
        原初的直觉 = 3551u,
        /// <summary>
        ///58级，回血，持续15s，冷却60s，84级hot
        /// </summary>
        泰然自若 = 3552u,
        /// <summary>
        ///62级，突进，可储存2层，冷却30s，88级可储存3层
        /// </summary>
        猛攻 = 7386u,
        /// <summary>
        ///64级，单体伤害能力，冷却30s，与群山隆起共享冷却
        /// </summary>
        动乱 = 7387u,
        /// <summary>
        ///68级，15%群盾，冷却90s
        ///另外，若自身处于“战栗”“戮罪”“原初的血气”状态，发动该技能会解除这些状态
        ///每解除一个状态，防护罩的效果量上升2%
        ///群体hot，持续15s
        ///76级群奶
        /// </summary>
        摆脱 = 7388u,
        /// <summary>
        ///70级，为自身附加3档原初的解放状态，持续15s，冷却60s
        ///原初的解放效果：兽魂技不消耗兽魂，且必定暴击并且直击，防击退
        ///续10s红斩
        ///追加效果：蛮荒崩裂预备，持续30s
        ///96级裂石飞环或地毁人亡命中后为自身附加1档原初的搏动状态，持续30s，积累3档后，原初的解放变为原初的怒震
        /// </summary>
        原初的解放 = 7389u,
        /// <summary>
        ///40级，aoe连击2，续30s红斩，74级获得20兽魂
        /// </summary>
        秘银暴风 = 16462u,
        /// <summary>
        ///强化aoe兽魂技
        /// </summary>
        混沌旋风 = 16463u,
        /// <summary>
        ///76级，目标减伤10%，双方吸血，持续8s，冷却25s
        ///目标减伤10%，持续4s
        ///目标单盾，持续20s
        /// </summary>
        原初的勇猛 = 16464u,
        /// <summary>
        ///强化单体兽魂技
        /// </summary>
        狂魂 = 16465u,
        /// <summary>
        ///82级，自身减伤10%，吸血，持续8s，冷却25s
        ///减伤10%，持续4s
        ///单盾，持续20s
        /// </summary>
        原初的血气 = 25751u,
        /// <summary>
        ///86级，aoe能力，冷却30s，与动乱共享冷却
        /// </summary>
        群山隆起 = 25752u,
        /// <summary>
        ///90级，突击，aoe，解放后可用
        /// </summary>
        蛮荒崩裂 = 25753u,
        /// <summary>
        ///
        /// </summary>
        戮罪 = 36923u,
        /// <summary>
        ///96级，伤害gcd
        /// </summary>
        原初的怒震 = 36924u,
        /// <summary>
        ///100级伤害gcd
        /// </summary>
        尽毁 = 36925u,
        /// <summary>
        ///减伤20%，持续20s，冷却90s，94级提高15%受治疗
        /// </summary>
        铁壁 = 7531u,
        /// <summary>
        ///
        /// </summary>
        挑衅 = 7533u,
        /// <summary>
        ///
        /// </summary>
        雪仇 = 7535u,
        /// <summary>
        ///
        /// </summary>
        退避 = 7537u,
        /// <summary>
        ///
        /// </summary>
        插言 = 7538u,
        /// <summary>
        ///
        /// </summary>
        下踢 = 7540u,
        /// <summary>
        ///
        /// </summary>
        亲疏自行 = 7548u,

    }



}
