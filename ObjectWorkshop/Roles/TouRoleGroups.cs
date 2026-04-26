namespace TownOfUs.Roles;

public static class TouRoleGroups
{
    public static RoleOptionsGroup CI { get; } = new("Crewmate Investigative Roles", RoleColors.Crewmate);
    public static RoleOptionsGroup CK { get; } = new("Crewmate Killing Roles", RoleColors.Crewmate);
    public static RoleOptionsGroup CP { get; } = new("Crewmate Protective Roles", RoleColors.Crewmate);
    public static RoleOptionsGroup CS { get; } = new("Crewmate Support Roles", RoleColors.Crewmate);
    public static RoleOptionsGroup CU { get; } = new("Crewmate Utility Roles", RoleColors.Crewmate);

    public static RoleOptionsGroup NA { get; } = new("Neutral Associative Roles", RoleColors.Neutral);
    public static RoleOptionsGroup NB { get; } = new("Neutral Benign Roles", RoleColors.Neutral);
    public static RoleOptionsGroup NC { get; } = new("Neutral Cataclysmic Roles", RoleColors.Neutral);
    public static RoleOptionsGroup NE { get; } = new("Neutral Evil Roles", RoleColors.Neutral);
    public static RoleOptionsGroup NP { get; } = new("Neutral Predator Roles", RoleColors.Neutral);

    public static RoleOptionsGroup ID { get; } = new("Infiltrator Disruption Roles", RoleColors.Infiltrator);
    public static RoleOptionsGroup IE { get; } = new("Infiltrator Evacuative Roles", RoleColors.Infiltrator);
    public static RoleOptionsGroup IK { get; } = new("Infiltrator Killing Roles", RoleColors.Infiltrator);
    public static RoleOptionsGroup IU { get; } = new("Infiltrator Utility Roles", RoleColors.Infiltrator);
}