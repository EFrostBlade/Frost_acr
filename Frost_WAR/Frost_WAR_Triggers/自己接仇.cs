using AEAssist;
using AEAssist.CombatRoutine.Trigger;
using AEAssist.GUI;
using AEAssist.Helper;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.LanguageHelpers;
using System.Numerics;

namespace Frost.Frost_WAR.Frost_WAR_Triggers;

public class 自己接仇 : ITriggerCond
{
    public string DisplayName => "WAR/自己接仇".Loc();

    public string Remark { get; set; }

    public bool Draw()
    {
        return false;
    }

    public bool Handle(ITriggerCondParams triggerCondParams)
    {
        IBattleChara? target = (IBattleChara)Core.Me.TargetObject;
        if ( target == null)
        {
            return false;
        }
        if (target.TargetObject == Core.Me)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}