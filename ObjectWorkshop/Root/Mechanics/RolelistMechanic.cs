using AmongUs.GameOptions;
using TownOfUs.Options;

namespace ObjectWorkshop.Mechanics;

public static class RolelistMechanic
{
    public static int InfiltratorCount;
    public static void GenerateRoleListAndApplyRoles(List<NetworkedPlayerInfo> infected)
    {
        var impostors = MiscUtils.GetImpostors(infected);
        var crewmates = MiscUtils.GetCrewmates(impostors);

        var rolesAssigned = new List<ushort>();

        var buckets = GetBuckets();
        int guaranteedCovenCount = buckets.Count(x => x is RoleListOption.RandomInfiltrator or /*RoleListOption.InfiltratorDeception or RoleListOption.InfiltratorKilling or RoleListOption.InfiltratorSupport or*/ RoleListOption.InfiltratorUtility);

        InfiltratorCount += guaranteedCovenCount;
        foreach (var bucket in buckets.OrderBy(x => x is RoleListOption.Any))
        {
            if (bucket is RoleListOption.CrewmateInvestigative) AssignCrewmateRole(rolesAssigned, Alignment.CrewmateInvestigative);
            //if (bucket is RoleListOption.CrewmateKilling) AssignCrewmateRole(rolesAssigned, Alignment.CrewmateKilling);
            //if (bucket is RoleListOption.CrewmateProtective) AssignCrewmateRole(rolesAssigned, Alignment.CrewmateProtective);
            //if (bucket is RoleListOption.CrewmateSupport) AssignCrewmateRole(rolesAssigned, Alignment.CrewmateSupport);
            if (bucket is RoleListOption.CrewmateUtility) AssignCrewmateRole(rolesAssigned, Alignment.CrewmateUtility);
            if (bucket is RoleListOption.RandomCrewmate) AssignCrewmateRole(rolesAssigned, Alignment.None, bucket);

            //if (bucket is RoleListOption.na) AssignNeutralRole(rolesAssigned, Alignment.NeutralBenign);
            if (bucket is RoleListOption.NeutralCataclysmic) AssignNeutralRole(rolesAssigned, Alignment.NeutralCataclysmic);
            //if (bucket is RoleListOption.NeutralEvil) AssignNeutralRole(rolesAssigned, Alignment.NeutralEvil);
            //if (bucket is RoleListOption.NeutralKilling) AssignNeutralRole(rolesAssigned, Alignment.NeutralKilling);
            //if (bucket is RoleListOption.RandomNeutral) AssignNeutralRole(rolesAssigned, Alignment.None, bucket);

            if (bucket is RoleListOption.InfiltratorDisruption) AssignInfiltratorRole(rolesAssigned, Alignment.InfiltratorDisruption);
            if (bucket is RoleListOption.InfiltratorEvacuative) AssignInfiltratorRole(rolesAssigned, Alignment.InfiltratorEvacuative);
            //if (bucket is RoleListOption.InfiltratorKilling) AssignInfiltratorRole(rolesAssigned, Alignment.InfiltratorKilling);
            if (bucket is RoleListOption.InfiltratorUtility) AssignInfiltratorRole(rolesAssigned, Alignment.InfiltratorUtility);
            if (bucket is RoleListOption.RandomInfiltrator) AssignInfiltratorRole(rolesAssigned, Alignment.None, bucket);

            if (bucket is RoleListOption.Any) AssignAnyRole(rolesAssigned);
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

    public static void AssignAnyRole(List<ushort> rolesAssigned)
    {
        int maxInfiltrator = (int)OptionGroupSingleton<InfiltratorOptions>.Instance.MaxInfiltrator;
        var allRoles = MiscUtils.AllRoles.Where(x => x is ICustomAURole && x is not ISpawnChange).ToList();

        if (InfiltratorCount >= maxInfiltrator) allRoles = MiscUtils.AllRoles.Where(x => x is ICustomAURole customRole && x is not ISpawnChange &&
            customRole.Faction != Faction.Infiltrator && !rolesAssigned.Contains(RoleId.Get(customRole.GetType()))).ToList();

        allRoles.AddRange(allRoles.AddDuplicateRolesToPool());
        var rolesAssignable = allRoles.ToList();

        bool gotRole = false;
        int attempts = 0;
        while (!gotRole && attempts < 50)
        {
            InfiltratorCount = rolesAssigned.Count(x => RoleManager.Instance.GetRole((RoleTypes)x) is ICustomAURole i && i.Faction == Faction.Infiltrator);
            if (rolesAssignable.Count == 0)
            {
                attempts++;
                rolesAssignable = allRoles.Where(x => !rolesAssigned.Contains(RoleId.Get(x.GetType()))).ToList();
                if (attempts < 5) OWPlugin.DebugLogMessage($"No more roles to assign, refreshing!", OWPlugin.MsgType.Warning);
            }

            var role = rolesAssignable.Random();
            var customRole = role as ICustomAURole;
            if (customRole != null && role != null)
            {
                var chance = customRole.GetChance();
                var count = customRole.GetCount();
                if (chance != null && count != null)
                {
                    if (CalculatedVoting.ChanceIsNull(chance))
                    {
                        var roleUSHORT = RoleId.Get(customRole.GetType());
                        
                        if (rolesAssigned.Count(x => x.UshortToRole().NiceName == role.NiceName) >= count) OWPlugin.DebugLogMessage($"Failed to assign {customRole.RoleName} because it has reached its max count!", OWPlugin.MsgType.Warning);
                        else if (InfiltratorCount > maxInfiltrator && customRole.Faction == Faction.Infiltrator) OWPlugin.DebugLogMessage($"Failed to assign {customRole.RoleName} because max Infiltrator members reached!");
                        else
                        {
                            if (customRole.Faction == Faction.Infiltrator) InfiltratorCount++;

                            gotRole = true;
                            rolesAssigned.Add(roleUSHORT);
                            OWPlugin.DebugLogMessage($"Assigned {customRole.RoleName}!");
                        }
                    }
                    else OWPlugin.DebugLogMessage($"Failed to assign {customRole.RoleName} because it failed to roll chance!");
                }
            }

            rolesAssignable.Remove(role);
        }
    }

    public static void AssignNeutralRole(List<ushort> rolesAssigned, Alignment alignment, RoleListOption bucket = RoleListOption.None)
    {
        var allRoles = MiscUtils.AllRoles.Where(x => x is ICustomAURole customRole && x is not ISpawnChange &&
            customRole.Alignment == alignment).ToList();

        //if (bucket == RoleListOption.RandomNeutral) allRoles = MiscUtils.AllRoles.Where(x => x is ICustomAURole customRole && x is not ISpawnChange && customRole.IsNeutral()).ToList();
        
        allRoles.AddRange(allRoles.AddDuplicateRolesToPool());
        var rolesAssignable = allRoles.ToList();

        bool gotRole = false;
        int attempts = 0;
        while (!gotRole && attempts < 50)
        {
            if (rolesAssignable.Count == 0)
            {
                attempts++;
                rolesAssignable = allRoles.Where(x => !rolesAssigned.Contains(RoleId.Get(x.GetType()))).ToList();
                if (attempts < 5) OWPlugin.DebugLogMessage($"No more roles to assign, refreshing!", OWPlugin.MsgType.Warning);
            }

            var role = rolesAssignable.Random();
            var customRole = role as ICustomAURole;
            if (customRole != null && role != null)
            {
                var chance = customRole.GetChance();
                var count = customRole.GetCount();
                if (chance != null && count != null)
                {
                    if (CalculatedVoting.ChanceIsNull(chance))
                    {
                        var roleUSHORT = RoleId.Get(customRole.GetType());
                        if (rolesAssigned.Count(x => x.UshortToRole().NiceName == role.NiceName) >= count) OWPlugin.DebugLogMessage($"Failed to assign {customRole.RoleName} because it has reached its max count!", OWPlugin.MsgType.Warning);
                        else
                        {
                            gotRole = true;
                            rolesAssigned.Add(roleUSHORT);
                            OWPlugin.DebugLogMessage($"Assigned {customRole.RoleName}!");
                        }
                    }
                    else OWPlugin.DebugLogMessage($"Failed to assign {customRole.RoleName} because it failed to roll chance!");
                }
            }

            rolesAssignable.Remove(role);
        }
    }

    public static void AssignCrewmateRole(List<ushort> rolesAssigned, Alignment alignment, RoleListOption bucket = RoleListOption.None)
    {
        var allRoles = MiscUtils.AllRoles.Where(x => x is ICustomAURole customRole && x is not ISpawnChange &&
            customRole.Alignment == alignment).ToList();

        if (bucket == RoleListOption.RandomCrewmate) allRoles = MiscUtils.AllRoles.Where(x => x is ICustomAURole customRole && x is not ISpawnChange && customRole.Faction == Faction.Crewmate).ToList();
        
        allRoles.AddRange(allRoles.AddDuplicateRolesToPool());
        var rolesAssignable = allRoles.ToList();

        bool gotRole = false;
        int attempts = 0;
        while (!gotRole && attempts < 50)
        {
            if (rolesAssignable.Count == 0)
            {
                attempts++;
                rolesAssignable = allRoles.Where(x => !rolesAssigned.Contains(RoleId.Get(x.GetType()))).ToList();
                if (attempts < 5) OWPlugin.DebugLogMessage($"No more roles to assign, refreshing!", OWPlugin.MsgType.Warning);
            }

            var role = rolesAssignable.Random();
            var customRole = role as ICustomAURole;
            if (customRole != null && role != null)
            {
                var chance = customRole.GetChance();
                var count = customRole.GetCount();
                if (chance != null && count != null)
                {
                    if (CalculatedVoting.ChanceIsNull(chance))
                    {
                        var roleUSHORT = RoleId.Get(customRole.GetType());
                        if (rolesAssigned.Count(x => x.UshortToRole().NiceName == role.NiceName) >= count) OWPlugin.DebugLogMessage($"Failed to assign {customRole.RoleName} because it has reached its max count!", OWPlugin.MsgType.Warning);
                        else
                        {
                            gotRole = true;
                            rolesAssigned.Add(roleUSHORT);
                            OWPlugin.DebugLogMessage($"Assigned {customRole.RoleName}!");
                        }
                    }
                    else OWPlugin.DebugLogMessage($"Failed to assign {customRole.RoleName} because it failed to roll chance!");
                }
            }

            rolesAssignable.Remove(role);
        }
    }

    public static void AssignInfiltratorRole(List<ushort> rolesAssigned, Alignment alignment, RoleListOption bucket = RoleListOption.None)
    {
        var allRoles = MiscUtils.AllRoles.Where(x => x is ICustomAURole customRole && x is not ISpawnChange &&
            customRole.Alignment == alignment).ToList();

        if (bucket == RoleListOption.RandomInfiltrator) allRoles = MiscUtils.AllRoles.Where(x => x is ICustomAURole customRole && x is not ISpawnChange && customRole.Faction == Faction.Infiltrator).ToList();
        
        allRoles.AddRange(allRoles.AddDuplicateRolesToPool());
        var rolesAssignable = allRoles.ToList();

        bool gotRole = false;
        int attempts = 0;
        while (!gotRole && attempts < 50)
        {
            if (rolesAssignable.Count == 0)
            {
                attempts++;
                rolesAssignable = allRoles.Where(x => !rolesAssigned.Contains(RoleId.Get(x.GetType()))).ToList();
                if (attempts < 5) OWPlugin.DebugLogMessage($"No more roles to assign, refreshing!", OWPlugin.MsgType.Warning);
            }

            var role = rolesAssignable.Random();
            var customRole = role as ICustomAURole;
            if (customRole != null && role != null)
            {
                var chance = customRole.GetChance();
                var count = customRole.GetCount();
                if (chance != null && count != null)
                {
                    if (CalculatedVoting.ChanceIsNull(chance))
                    {
                        var roleUSHORT = RoleId.Get(customRole.GetType());
                        if (rolesAssigned.Count(x => x.UshortToRole().NiceName == role.NiceName) >= count) OWPlugin.DebugLogMessage($"Failed to assign {customRole.RoleName} because it has reached its max count!", OWPlugin.MsgType.Warning);
                        else
                        {
                            gotRole = true;
                            rolesAssigned.Add(roleUSHORT);
                            OWPlugin.DebugLogMessage($"Assigned {customRole.RoleName}!");
                        }
                    }
                    else OWPlugin.DebugLogMessage($"Failed to assign {customRole.RoleName} because it failed to roll chance!");
                }
            }

            rolesAssignable.Remove(role);
        }
    }

    public static List<RoleBehaviour> AddDuplicateRolesToPool(this List<RoleBehaviour> list)
    {
        var allRoles = new List<RoleBehaviour>();
        if (!OptionGroupSingleton<RoleGenOptions>.Instance.LegacyRoleGen) return allRoles;

        foreach (var role in list.ToList())
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

        return allRoles;
    }

    public static List<RoleListOption> GetBuckets()
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

        return buckets;
    }

    public static RoleBehaviour UshortToRole(this ushort x)
    {
        return RoleManager.Instance.GetRole((RoleTypes)x);
    }
}