using AEAssist.CombatRoutine.Trigger.Node;
using AEAssist.CombatRoutine.Trigger;
using System.Reflection;
using AEAssist.Helper;
namespace ScriptTest;

public class 以太税位置读条 : ITriggerScript
{
    public bool Check(ScriptEnv scriptEnv, ITriggerCondParams condParams)
    {
        if (condParams is not EnemyCastSpellCondParams { SpellId: 40972 } spell) return false;
        if (spell.SpellId == 40972)
        {
            //中间 <99.99231, -0.015258789, 79.972534>
            if (spell.CastPos.X == 99.99231 && spell.CastPos.Y == -0.015258789 && spell.CastPos.Z == 79.972534)
            {
                scriptEnv.KV["以太税位置"] = 0;
            }

            return true;
        }
        return false;
    }
}