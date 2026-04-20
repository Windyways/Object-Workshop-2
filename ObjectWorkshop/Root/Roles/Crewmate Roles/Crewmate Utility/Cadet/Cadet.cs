using System.Text;
using Il2CppInterop.Runtime.Attributes;
using UnityEngine;

namespace ObjectWorkshop.Roles;

public sealed class Cadet(IntPtr cppPtr) : CrewmateRole(cppPtr), ICustomAURole, IWikiDiscoverable
{
    public string RoleName { get; set; } = "Cadet";
    public string RoleDescription => "";
    public string RoleLongDescription => "The vanilla Crewmate.";
    public Color RoleColor { get; set; } = RoleColors.Crewmate;

    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public Faction Faction { get; set; } = Faction.Crewmate;
    public CalculatedFaction CalculatedFaction { get; set; } = CalculatedFaction.Crewmate;
    public Alignment Alignment => Alignment.CrewmateUtility;

    public string RevealText => "is a member of The Skeld.";
    public string Description => "Eject and dispose of all prime evildoers.";
    public string Intro => "You are an astronaut that has tasks to fulfil.";
    public string VictoryCondition => "Dispose of all evil. You will win with other Crewmate members.";

    public CustomRoleConfiguration Configuration => new(this)
    {

    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ICustomAURole.SetNewTabText(this);
    }

    public string GetAdvancedDescription()
    {
        return
            $"{Intro}\n\n{Description}\n\n" +
            $"<color=#00ff00>Victory Condition:</color>\n" +
            $"{VictoryCondition}" +
            MiscUtils.AppendOptionsText(GetType());
    }
}