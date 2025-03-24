using AEAssist.CombatRoutine.Trigger.Node;
using AEAssist.CombatRoutine.Trigger;
using AEAssist.Helper;
using AEAssist.CombatRoutine.Module;
using System.Numerics;
using AEAssist;
using Dalamud.Game.ClientState.Objects.Types;
using AEAssist.Extension;
using System.Threading;

namespace ScriptTest;

public class 落地 : ITriggerScript
{
    public bool Completed = false;

    public readonly object _executionLock = new object();

    public bool Check(ScriptEnv scriptEnv, ITriggerCondParams condParams)
    {
        lock (_executionLock)
        {
            bool anyHasAura = false;
            foreach (IBattleChara partyer in PartyHelper.Party)
            {
                if (partyer == null)
                    continue;
                if (partyer.HasAura(3770))
                {
                    anyHasAura = true;
                    break;
                }
            }
            Completed = !anyHasAura;
        }
        return Completed;
    }
}
