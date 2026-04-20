using AmongUs.GameOptions;
using Hazel;
using Reactor.Utilities.Extensions;
using TownOfUs.Events.TouEvents;
using TownOfUs.Options;
using Random = UnityEngine.Random;

namespace ObjectWorkshop.Patches;

[HarmonyPatch]
public static class TouRoleManagerPatches
{
    private static readonly List<RoleTypes> CrewmateGhostRolePool = [];
    private static readonly List<RoleTypes> ImpostorGhostRolePool = [];
    private static readonly List<RoleTypes> CustomGhostRolePool = [];

    public static bool ReplaceRoleManager;
    private static List<int> LastImps { get; set; } = [];

    private static void GhostRoleSetup()
    {
        // var ghostRoles = RoleManager.Instance.AllRoles.Where(x => x.IsDead);
        var ghostRoles = MiscUtils.GetRegisteredGhostRoles();

        if (OWPlugin.IsDevBuild) Logger<OWPlugin>.Warning($"GhostRoleSetup - ghostRoles Count: {ghostRoles.Count()}");
        CrewmateGhostRolePool.Clear();
        ImpostorGhostRolePool.Clear();
        CustomGhostRolePool.Clear();

        foreach (var role in ghostRoles)
        {
            if (OWPlugin.IsDevBuild) Logger<OWPlugin>.Warning($"GhostRoleSetup - ghostRoles role NiceName: {role.NiceName}");
            var data = MiscUtils.GetAssignData(role.Role);

            switch (data.Chance)
            {
                case 100:
                    {
                        if (data.Count > 0)
                        {
                            if (role is ICustomRole { Team: ModdedRoleTeams.Custom })
                            {
                                CustomGhostRolePool.Add(role.Role);
                            }
                            else
                            {
                                switch (role.TeamType)
                                {
                                    case RoleTeamTypes.Crewmate:
                                        CrewmateGhostRolePool.Add(role.Role);
                                        break;
                                    case RoleTeamTypes.Impostor:
                                        ImpostorGhostRolePool.Add(role.Role);
                                        break;
                                }
                            }
                        }

                        break;
                    }
                case > 0:
                    {
                        if (data.Count > 0 && HashRandom.Next(101) < data.Chance)
                        {
                            if (role is ICustomRole { Team: ModdedRoleTeams.Custom })
                            {
                                CustomGhostRolePool.Add(role.Role);
                            }
                            else
                            {
                                switch (role.TeamType)
                                {
                                    case RoleTeamTypes.Crewmate:
                                        CrewmateGhostRolePool.Add(role.Role);
                                        break;
                                    case RoleTeamTypes.Impostor:
                                        ImpostorGhostRolePool.Add(role.Role);
                                        break;
                                }
                            }
                        }

                        break;
                    }
            }
        }
    }

    public static void PerformAllAnyRoleGeneration(List<NetworkedPlayerInfo> infected)
    {
        var impostors = MiscUtils.GetImpostors(infected);
        var crewmates = MiscUtils.GetCrewmates(impostors);

        var rolesAssigned = new List<ushort>();
        var allRoles = MiscUtils.AllRoles.Where(x => x is ICustomAURole && x is not ISpawnChange).ToList();

        // Here we try to add duplicate roles if there are more than 1 of it.
        foreach (var role in allRoles.ToList())
        {
            if (role is ICustomAURole customRole)
            {
                var count = customRole.GetCount();
                if (count != null)
                {
                    for (int i = 1; i < count; i++)
                    {
                        allRoles.Add(role);
                    }
                }
            }
        }

        var rolesAssignable = allRoles.ToList();
        var players = GameData.Instance.PlayerCount;

        // --- INFILTRATOR ---
        int maxInfiltrator = (int)OptionGroupSingleton<InfiltratorOptions>.Instance.MaxInfiltrator;
        int infiltratorCount = 0;

        int attempts = 0;
        while (rolesAssigned.Count < players && attempts < 1000)
        {
            if (rolesAssignable.Count == 0)
            {
                rolesAssignable = allRoles.Where(x => !rolesAssigned.Contains(RoleId.Get(x.GetType()))).ToList();
                OWPlugin.DebugLogMessage($"No more roles to assign, refreshing!", OWPlugin.MsgType.Warning);
            }

            var role = rolesAssignable.Random();
            var customRole = role as ICustomAURole;
            if (customRole != null)
            {
                var chance = customRole.GetChance();
                var count = customRole.GetCount();
                if (chance != null && count != null)
                {
                    if (CalculatedVoting.ChanceIsNull(chance))
                    {
                        if (infiltratorCount >= maxInfiltrator && customRole.Faction == Faction.Infiltrator) OWPlugin.DebugLogMessage($"Failed to assign {customRole.RoleName} because max Infiltrator members reached!");
                        else
                        {
                            if (customRole.Faction == Faction.Infiltrator) infiltratorCount++;

                            rolesAssigned.Add(RoleId.Get(customRole.GetType()));
                            OWPlugin.DebugLogMessage($"Assigned {customRole.RoleName}!");
                        }
                    }
                    else OWPlugin.DebugLogMessage($"Failed to assign {customRole.RoleName} because it failed to roll chance!");
                }
            }

            rolesAssignable.Remove(role);

            attempts++;
        }

        foreach (var role in rolesAssigned)
        {
            var num = HashRandom.FastNext(crewmates.Count);
            var player = crewmates[num];

            player.RpcSetRole((RoleTypes)role);

            crewmates.RemoveAt(num);

            if (OWPlugin.IsDevBuild) Logger<OWPlugin>.Warning($"Assigning {RoleManager.Instance.GetRole((RoleTypes)role).NiceName} to {player.Data.PlayerName}.");
        }

        // Assign vanilla roles to anyone who did not receive a role.
        foreach (var player in crewmates) player.RpcSetRole((RoleTypes)RoleId.Get<Cadet>());
        foreach (var player in impostors) player.RpcSetRole((RoleTypes)RoleId.Get<Marauder>());
    }

    public static void AssignTargets()
    {
        foreach (var role in MiscUtils.AllRoles.Where(x => x is IAssignableTargets)
                     .OrderBy(x => (x as IAssignableTargets)!.Priority))
        {
            if (role is IAssignableTargets assignRole)
            {
                assignRole.AssignTargets();
            }
        }

        foreach (var modifier in MiscUtils.AllModifiers.Where(x => x is IAssignableTargets)
                     .OrderBy(x => (x as IAssignableTargets)!.Priority))
        {
            if (modifier is IAssignableTargets assignMod)
            {
                assignMod.AssignTargets();
            }
        }

        GhostRoleSetup();
    }

    [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
    [HarmonyPrefix]
    [HarmonyPriority(Priority.Last)]
    public static bool SelectRolesPatch(RoleManager __instance)
    {
        if (OWPlugin.IsDevBuild) Logger<OWPlugin>.Error($"RoleManager.SelectRoles - ReplaceRoleManager: {ReplaceRoleManager}");

        if (TutorialManager.InstanceExists || ReplaceRoleManager)
        {
            return true;
        }

        //Logger<OWPlugin>.Error($"RoleManager.SelectRoles 2");

        var players = GameData.Instance.AllPlayers.ToArray().ToList();
        players.Shuffle();

        var impCount = GameOptionsManager.Instance.CurrentGameOptions.GetAdjustedNumImpostors(players.Count);
        List<NetworkedPlayerInfo> infected = [];

        infected.AddRange(players.Take(impCount));

        LastImps = [.. infected.Select(x => x.ClientId)];

        if (AllAnyMode()) PerformAllAnyRoleGeneration(infected);
        else RolelistMechanic.GenerateRoleListAndApplyRoles(infected); //AssignRolesFromRoleList(infected);

        AssignTargets();

        return false;
    }

    public static bool AllAnyMode()
    {
        var playerCount = GameData.Instance.AllPlayers.Count;
        var opts = OptionGroupSingleton<RoleOptions>.Instance;
        List<RoleListOption> buckets =
        [
            (RoleListOption)opts.Slot1.Value, (RoleListOption)opts.Slot2.Value, (RoleListOption)opts.Slot3.Value,
            (RoleListOption)opts.Slot4.Value
        ];

        var anySlots = 0;

        if (playerCount > 4) buckets.Add((RoleListOption)opts.Slot5.Value);
        if (playerCount > 5) buckets.Add((RoleListOption)opts.Slot6.Value);
        if (playerCount > 6) buckets.Add((RoleListOption)opts.Slot7.Value);
        if (playerCount > 7) buckets.Add((RoleListOption)opts.Slot8.Value);
        if (playerCount > 8) buckets.Add((RoleListOption)opts.Slot9.Value);
        if (playerCount > 9) buckets.Add((RoleListOption)opts.Slot10.Value);
        if (playerCount > 10) buckets.Add((RoleListOption)opts.Slot11.Value);
        if (playerCount > 11) buckets.Add((RoleListOption)opts.Slot12.Value);
        if (playerCount > 12) buckets.Add((RoleListOption)opts.Slot13.Value);
        if (playerCount > 13) buckets.Add((RoleListOption)opts.Slot14.Value);
        if (playerCount > 14) buckets.Add((RoleListOption)opts.Slot15.Value);
        if (playerCount > 15)
        {
            for (var i = 0; i < playerCount - 15; i++) buckets.Add(RoleListOption.Any);
        }

        foreach (var roleOption in buckets)
            if (roleOption == RoleListOption.Any) anySlots += 1;

        return anySlots == playerCount;
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSetRole))]
    [HarmonyPrefix]
    public static bool RpcSetRolePatch(PlayerControl __instance, [HarmonyArgument(0)] RoleTypes roleType,
        [HarmonyArgument(1)] bool canOverrideRole = false)
    {
        if (AmongUsClient.Instance.AmClient)
        {
            __instance.StartCoroutine(__instance.CoSetRole(roleType, canOverrideRole));
        }

        var messageWriter =
            AmongUsClient.Instance.StartRpcImmediately(__instance.NetId, (byte)RpcCalls.SetRole, SendOption.Reliable);
        messageWriter.Write((ushort)roleType);
        messageWriter.Write(canOverrideRole);
        AmongUsClient.Instance.FinishRpcImmediately(messageWriter);

        var changeRoleEvent = new ChangeRoleEvent(__instance, null, RoleManager.Instance.GetRole(roleType));
        MiraEventManager.InvokeEvent(changeRoleEvent);

        return false;
    }

    [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.AssignRoleOnDeath))]
    [HarmonyPrefix]
    public static bool AssignRoleOnDeathPatch(RoleManager __instance, PlayerControl player, bool specialRolesAllowed)
    {
        // Note: I know this is a like for like recreation of the AssignRoleOnDeath function but for some reason
        // the original won't spawn the Phantom and just spawns Neutral Ghost instead

        if (player == null || !player.Data.IsDead) // Logger<OWPlugin>.Message($"AssignRoleOnDeathPatch - !player.Data.IsDead: '{!player.Data.IsDead}'");
        {
            return false;
        }

        /*if (specialRolesAllowed && !player.HasModifier<BasicGhostModifier>())
            // Logger<OWPlugin>.Message($"AssignRoleOnDeathPatch - !player.Data.Role.IsImpostor: '{!player.Data.Role.IsImpostor}' specialRolesAllowed: {specialRolesAllowed}");
        {
            RoleManager.TryAssignSpecialGhostRoles(player, player.IsImpostor());
        }*/

        if (!RoleManager.IsGhostRole(player.Data.Role.Role))
        // Logger<OWPlugin>.Message($"AssignRoleOnDeathPatch - !RoleManager.IsGhostRole(player.Data.Role.Role): '{!RoleManager.IsGhostRole(player.Data.Role.Role)}'");
        {
            player.RpcSetRole(player.Data.Role.DefaultGhostRole);
        }

        return false;
    }

    [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.TryAssignSpecialGhostRoles))]
    [HarmonyPrefix]
    public static bool TryAssignSpecialGhostRolesPatch(RoleManager __instance, PlayerControl player)
    {
        if (OWPlugin.IsDevBuild) Logger<OWPlugin>.Warning($"TryAssignSpecialGhostRolesPatch - Player: '{player.Data.PlayerName}'");
        var ghostRole = RoleTypes.CrewmateGhost;

        if (player.IsCrewmate() && CrewmateGhostRolePool.Count > 0)
        {
            ghostRole = CrewmateGhostRolePool.TakeFirst();
        }
        else if (player.IsImpostor() && ImpostorGhostRolePool.Count > 0)
        {
            ghostRole = ImpostorGhostRolePool.TakeFirst();
        }
        else if (player.IsNeutral() && CustomGhostRolePool.Count > 0)
        {
            ghostRole = CustomGhostRolePool.TakeFirst();
        }

        if (ghostRole != RoleTypes.CrewmateGhost && ghostRole != RoleTypes.ImpostorGhost &&
            ghostRole != (RoleTypes)RoleId.Get<NeutralGhostRole>())
        // var newRole = RoleManager.Instance.GetRole(ghostRole);
        // Logger<OWPlugin>.Message($"TryAssignSpecialGhostRolesPatch - ghostRoles role: {newRole.NiceName}");
        {
            player.RpcChangeRole((ushort)ghostRole);
        }

        return false;
    }

    //[HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SetRole))]
    //[HarmonyPostfix]
    //public static void SetRolePatch(RoleManager __instance, [HarmonyArgument(0)] PlayerControl targetPlayer, [HarmonyArgument(1)] RoleTypes roleType)
    //{
    //    GameHistory.RegisterRole(targetPlayer, targetPlayer.Data.Role);
    //}
    [HarmonyPatch(typeof(IGameOptionsExtensions), nameof(IGameOptionsExtensions.GetAdjustedNumImpostors))]
    [HarmonyPrefix]
    public static bool GetAdjustedImposters(IGameOptions __instance, ref int __result)
    {
        if (GameOptionsManager.Instance.CurrentGameOptions.GameMode == GameModes.HideNSeek) return true;

        __result = 0;
        return false;
    }
}