using AEAssist.CombatRoutine.Trigger.Node;
using AEAssist.CombatRoutine.Trigger;
using AEAssist.Helper;
using AEAssist.CombatRoutine.Module;
namespace ScriptTest;

public class 左右刀判断 : ITriggerScript
{
    public bool Check(ScriptEnv scriptEnv, ITriggerCondParams condParams)
    {
        if (condParams is not EnemyCastSpellCondParams { SpellId: 40990 or 40992 } spell) return false;
        if (spell.SpellId == 40990)
        {
            AI.Instance.TriggerlineData.Variable["左右刀"] = 0;
            LogHelper.Print("先打左");
            return true;
        }
        if (spell.SpellId == 40992)
        {
            AI.Instance.TriggerlineData.Variable["左右刀"] = 1;
            LogHelper.Print("先打右");
            return true;
        }
        return false;
    }
}