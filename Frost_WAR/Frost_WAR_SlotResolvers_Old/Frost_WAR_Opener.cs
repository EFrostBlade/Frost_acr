using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.CombatRoutine.Module.Opener;
using AEAssist.Extension;
using AEAssist.Helper;
using Frost.Frost_WAR.Frost_WAR_Data;
using Frost.Frost_WAR.Frost_WAR_Setting;

namespace Frost.Frost_WAR.Frost_WAR_SlotResolvers
{
    public class Frost_WAR_Opener : IOpener
    {
        public void InitCountDown(CountDownHandler countDownHandler)
        {

            if (Frost_WAR_Settings.Instance.飞斧开怪 == true)
            {
                countDownHandler.AddAction(1000, (uint)WARActionID.飞斧,SpellTargetType.Target);
            }
        }
        public List<Action<Slot>> Sequence { get; } = new()//起手具体的队列，序号从0开始
        {
            //Step0
        };
    }
}
