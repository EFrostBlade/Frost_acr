using AEAssist.CombatRoutine.Trigger.Node;
using AEAssist.CombatRoutine.Trigger;
using System.Reflection;
using AEAssist.Helper;
namespace ScriptTest;

public class 以太税位置map : ITriggerScript
{
    public bool Check(ScriptEnv scriptEnv, ITriggerCondParams condParams)
    {
        if (condParams is not OnMapEffectCreateEvent map) return false;
        return false;
    }
}