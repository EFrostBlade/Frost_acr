using AEAssist;
using AEAssist.CombatRoutine.Trigger;
using AEAssist.GUI;
using AEAssist.Helper;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.LanguageHelpers;
using System.Numerics;

namespace Frost.Frost_WAR.Frost_WAR_Triggers;

public class 对位挑衅 : ITriggerCond
{
    public string DisplayName => "WAR/对位挑衅".Loc();

    public string Remark { get; set; }

    public bool Draw()
    {
        return false;
    }

    public bool Handle(ITriggerCondParams triggerCondParams)
    {
        IBattleChara? anotherTank = PartyHelper.GetAnotherTank(Core.Me);
        IBattleChara? target = (IBattleChara)Core.Me.TargetObject;
        if (anotherTank == null || target == null)
        {
            return false;
        }
        if (target.TargetObject == anotherTank)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}