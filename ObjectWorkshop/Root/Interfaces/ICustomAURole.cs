using Il2CppInterop.Runtime.Attributes;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace ObjectWorkshop.Interfaces;

public interface ICustomAURole : ICustomRole
{
    string RoleName { get; set; }
    Color RoleColor { get; set; }

    Faction Faction { get; set; }
    Alignment Alignment { get; }
    CalculatedFaction CalculatedFaction { get; set; }

    string RevealText { get; }
    string Description { get; }
    string Intro { get; }
    string VictoryCondition { get; }

    public virtual bool MetWinCon => false;
    public virtual string YouAreText
    {
        get
        {
            var prefix = " a";
            if (RoleName.StartsWithVowel()) prefix = " an";
            if (Configuration.MaxRoleCount is 0 or 1) prefix = " the";
            if (RoleName.StartsWith("the", StringComparison.OrdinalIgnoreCase)) prefix = "";
            return $"You are{prefix}";
        }
    }

    RoleOptionsGroup ICustomRole.RoleOptionsGroup
    {
        get
        {
            if (Alignment == Alignment.CrewmateInvestigative) return TouRoleGroups.CI;
            if (Alignment == Alignment.CrewmateKilling) return TouRoleGroups.CK;
            if (Alignment == Alignment.CrewmateProtective) return TouRoleGroups.CP;
            if (Alignment == Alignment.CrewmateSupport) return TouRoleGroups.CS;
            if (Alignment == Alignment.CrewmateUtility) return TouRoleGroups.CU;

            if (Alignment == Alignment.NeutralAssociative) return TouRoleGroups.NA;
            if (Alignment == Alignment.NeutralBenign) return TouRoleGroups.NB;
            if (Alignment == Alignment.NeutralCataclysmic) return TouRoleGroups.NC;
            if (Alignment == Alignment.NeutralEvil) return TouRoleGroups.NE;
            if (Alignment == Alignment.NeutralPredator) return TouRoleGroups.NP;

            if (Alignment == Alignment.InfiltratorDisruption) return TouRoleGroups.ID;
            if (Alignment == Alignment.InfiltratorEvacuative) return TouRoleGroups.IE;
            if (Alignment == Alignment.InfiltratorMilitant) return TouRoleGroups.IM;
            if (Alignment == Alignment.InfiltratorUtility) return TouRoleGroups.IU;

            return Team switch
            {
                ModdedRoleTeams.Crewmate => TouRoleGroups.CS,
                ModdedRoleTeams.Impostor => TouRoleGroups.IU,
                _ => TouRoleGroups.NE
            };
        }
    }

    bool WinConditionMet()
    {
        return false;
    }

    /// <summary>
    ///     LobbyStart - Called for each role when a lobby begins.
    /// </summary>
    void LobbyStart()
    {
    }

    public static StringBuilder SetNewTabText(ICustomRole role)
    {
        var alignment = role is ICustomAURole customRole
            ? customRole.Alignment.ToSpacedString().ApplyKeywords()
            : "Custom";

        var prefix = " a";
        if (role.RoleName.StartsWithVowel()) prefix = " an";
        if (role.Configuration.MaxRoleCount is 0 or 1) prefix = " the";
        if (role.RoleName.StartsWith("the", StringComparison.OrdinalIgnoreCase)) prefix = "";

        var stringB = new StringBuilder();
        stringB.AppendLine(CultureInfo.InvariantCulture,
            $"{role.RoleColor.ToTextColor()}You are{prefix}<b> {role.RoleName}.</b></color>");
        stringB.AppendLine(CultureInfo.InvariantCulture, $"<size=60%>Alignment: <b>{alignment}</color></b></size>");
        stringB.Append("<size=70%>");
        // stringB.AppendLine(CultureInfo.InvariantCulture, $"{role.RoleLongDescription}");

        return stringB;
    }

    public static StringBuilder SetDeadTabText(ICustomRole role)
    {
        var alignment = role is ICustomAURole customRole
            ? "<color=#ffffff" + customRole.Alignment.ToSpacedString().ApplyKeywords()
            : "Custom";

        var prefix = " a";
        if (role.RoleName.StartsWithVowel()) prefix = " an";
        if (role.Configuration.MaxRoleCount is 0 or 1) prefix = " the";
        if (role.RoleName.StartsWith("the", StringComparison.OrdinalIgnoreCase)) prefix = "";

        var stringB = new StringBuilder();
        stringB.AppendLine(CultureInfo.InvariantCulture,
            $"{role.RoleColor.ToTextColor()}You were{prefix}<b> {role.RoleName}.</b></color>");
        stringB.AppendLine(CultureInfo.InvariantCulture, $"<size=60%>Alignment: <b>{alignment}</color></b></size>");
        stringB.Append("<size=70%>");
        stringB.AppendLine(CultureInfo.InvariantCulture, $"{role.RoleLongDescription}");

        return stringB;
    }

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return SetNewTabText(this);
    }

    void Function(PlayerControl target, int Button)
    {
    }


    void Role_OnMeetingStart()
    {
    }

    void Role_OnRoundStart()
    {
    }

    void Role_AfterMurder(PlayerControl killer, PlayerControl victim)
    {
    }

    void Role_OnEjection(PlayerControl? ejected, ExileController exileController)
    {
    }

    void Role_OnVisitFail(PlayerControl visitor, PlayerControl target, bool isAttacking, bool isVisiting, int blockedVisit)
    {
    }

    void Role_OnDeath(PlayerControl? player)
    {
    }
}