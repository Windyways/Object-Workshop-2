using Random = UnityEngine.Random;

namespace ObjectWorkshop.MCI.SmartMCI;

public static class CalculatedVoting
{
    public static int GlobalSkipThreshold = 12;
    public static byte SkipVote(PlayerControl player, MeetingHud __instance)
    {
        __instance.CmdCastVote(player.PlayerId, __instance.SkipVoteButton.TargetPlayerId);
        return __instance.SkipVoteButton.TargetPlayerId;
    }

    public static byte RandomVote(PlayerControl player, MeetingHud __instance, List<PlayerControl> validTargets, bool canSkip = true)
    {
        bool skip = Random.RandomRangeInt(-1, validTargets.Count) == -1;
        if (skip && canSkip) return SkipVote(player, __instance);

        PlayerControl newTarget = validTargets[Random.RandomRangeInt(0, validTargets.Count)];
        validTargets.Remove(newTarget);
        __instance.CmdCastVote(player.PlayerId, newTarget.PlayerId);
        return newTarget.PlayerId;
    }

    public static byte TryCastVote(PlayerControl player, MeetingHud __instance, byte target)
    {
        __instance.CmdCastVote(player.PlayerId, target);
        return target;
    }

    public static byte TryCastVote(PlayerControl player, MeetingHud __instance, PlayerControl target)
    {
        if (target == null) return SkipVote(player, __instance);

        __instance.CmdCastVote(player.PlayerId, target.PlayerId);
        return target.PlayerId;
    }

    public static void DoVotes(MeetingHud __instance)
    {
        declaredCrewmateTarget = (byte.MinValue, false);
        lastInfiltratorVoteTarget = (byte.MinValue, false);

        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            var validPlayersCrewmate = PlayerControl.AllPlayerControls.ToArray().Where(x =>
                !x.HasDied() && x != player && !x.HasModifier<TI>(x => !x.Player.HasModifier<Suspicion>()) &&
                !(x.TryGetModifier<Confirmed>(out var confirmed) && confirmed.IsConfirmed()) && !x.HasModifier<SoftCleared>() &&
                !(x.Is(Faction.Crewmate) && x.HasModifier<GlobalReveal>()) &&
                !(x.Data.Role is Peacock peacock && peacock.Player.GetAssociate() == player)).ToList();

            var validPlayersInfiltrator = PlayerControl.AllPlayerControls.ToArray().Where(x =>
                !x.HasDied() && !x.Is(Faction.Infiltrator) &&
                !(x.Data.Role is Peacock peacock && peacock.Player.GetAssociate() == player)).ToList();

            if (!player.HasDied())
            {
                var alivePlayers = PlayerControl.AllPlayerControls.ToArray().Where(x => !x.HasDied() && x != player).ToList();

                byte voted = 0;
                if (player.Is(Faction.Infiltrator)) voted = InfiltratorVoting(player, __instance, validPlayersInfiltrator);
                else if (player.Is(Faction.Crewmate)) voted = CrewmateVoting(player, __instance, validPlayersCrewmate);
                else if (player.Data.Role is Shikari shikari) voted = ShikariVoting(shikari, __instance);
                else if (player.Data.Role is Peacock peacock) voted = PeacockVoting(peacock, __instance);
                else if (player.Is(Alignment.NeutralPredator)) voted = RandomVote(player, __instance, alivePlayers, alivePlayers.Count > GlobalSkipThreshold);
                else if (player.Is(Alignment.NeutralEvil)) voted = RandomVote(player, __instance, alivePlayers, alivePlayers.Count > GlobalSkipThreshold);
                else if (player.Is(Alignment.NeutralBenign)) voted = RandomVote(player, __instance, alivePlayers, alivePlayers.Count > GlobalSkipThreshold);

                if (voted == MeetingHud.Instance.SkipVoteButton.TargetPlayerId) AUS_AfterVoteEvent.RoleFunctionOnSkip(player);
                else AUS_AfterVoteEvent.RoleFunctionOnVote(player, MiscUtils.PlayerById(voted));
            }
        }
    }

    /// <summary>
    /// If a player is Confirmed Evil, vote them.
    /// If a player witnesses a kill, vote who they saw kill with a % chance. If failed, vote the witness.
    /// If there are less than 7 players, vote a random player that isn't yourself.
    /// Abstain otherwise.
    /// </summary>
    public static (byte, bool) declaredCrewmateTarget = (byte.MinValue, false);
    public static byte CrewmateVoting(PlayerControl player, MeetingHud __instance, List<PlayerControl> validPlayers)
    {
        var allPlayers = PlayerControl.AllPlayerControls.ToArray().Where(x => !x.HasDied()).ToList();

        var seenKill = SeenKill.GetAll();
        var confirmedEvil = ConfirmedEvil.GetAll();
        var suspicion = ModifierUtils.GetActiveModifiers<Suspicion>(x => validPlayers.Contains(x.Player))
            .Where(x => !x.Player.HasDied())
            .OrderByDescending(x => x.VotedChance).ThenByDescending(x => Random.value).FirstOrDefault();

        if (declaredCrewmateTarget.Item2 && ChanceIs(80)) return TryCastVote(player, __instance, declaredCrewmateTarget.Item1);
        else if (confirmedEvil != null) declaredCrewmateTarget = (TryCastVote(player, __instance, confirmedEvil.Player), true);
        else if (suspicion != null && ChanceIs(suspicion.VotedChance)) declaredCrewmateTarget = (TryCastVote(player, __instance, suspicion.Player), true);
        else if (seenKill != null)
        {
            var a = seenKill.killer;
            var b = seenKill.Player;

            int suspicionA = SeenKill.GetSuspicion(a);
            int suspicionB = SeenKill.GetSuspicion(b);

            // Bias by the witness credibility
            if (ChanceIs(seenKill.voteChance)) suspicionA += 25;
            else suspicionB += 25;

            if (suspicionA > suspicionB) return TryCastVote(player, __instance, a);
            else if (suspicionB > suspicionA) return TryCastVote(player, __instance, b);
            else return TryCastVote(player, __instance, UnityEngine.Random.value < 0.5f ? a : b);
        }
        else if (validPlayers.Count > 0) return RandomVote(player, __instance, validPlayers, allPlayers.Count >= GlobalSkipThreshold);
        else return SkipVote(player, __instance);

        return declaredCrewmateTarget.Item1;
    }

    /// <summary>
    /// Infiltrator has a 30% chance to vote with each other.
    /// If a Infiltrator is caught, vote the accuser.
    /// If there are less than 7 players, vote a random Non-Infiltrator.
    /// Abstain otherwise.
    /// </summary>
    public static (byte, bool) lastInfiltratorVoteTarget = (byte.MinValue, false);
    public static byte InfiltratorVoting(PlayerControl player, MeetingHud __instance, List<PlayerControl> validPlayers)
    {
        var allPlayers = PlayerControl.AllPlayerControls.ToArray().Where(x => !x.HasDied()).ToList();

        var seenKill = SeenKill.GetAll();

        var suspicion = Suspicion.GetAll();
        if (suspicion != null && suspicion.Count >= 2)
        {
            // If one is Infiltrator and one isn't -> vote the non-Infiltrator
            var nonInfiltratorTarget = suspicion.FirstOrDefault(p => !p.Player.Is(Faction.Infiltrator));
            if (nonInfiltratorTarget != null)
            {
                lastInfiltratorVoteTarget = (TryCastVote(player, __instance, nonInfiltratorTarget.Player), true);
                return lastInfiltratorVoteTarget.Item1;
            }
        }

        if (lastInfiltratorVoteTarget.Item2 && ChanceIs(30)) return TryCastVote(player, __instance, lastInfiltratorVoteTarget.Item1);
        else if (seenKill != null && !seenKill.killer.Is(Faction.Infiltrator) && !seenKill.Player.HasDied()) lastInfiltratorVoteTarget = (TryCastVote(player, __instance, seenKill.killer.PlayerId), true);
        else if (validPlayers.Count > 0) lastInfiltratorVoteTarget = (RandomVote(player, __instance, validPlayers, allPlayers.Count >= GlobalSkipThreshold), true);
        else lastInfiltratorVoteTarget = (SkipVote(player, __instance), true);

        return lastInfiltratorVoteTarget.Item1;
    }

    public static byte ShikariVoting(Shikari shikari, MeetingHud __instance)
    {
        var player = shikari.Player;
        var allPlayers = PlayerControl.AllPlayerControls.ToArray().Where(x => !x.HasDied()).ToList();
        var validPlayers = PlayerControl.AllPlayerControls.ToArray().Where(x =>
            !x.HasDied() && x != player && !shikari.MarkedPlayers.Contains(x)).ToList();
        
        if (validPlayers.Count > 0) return RandomVote(player, __instance, validPlayers, allPlayers.Count >= GlobalSkipThreshold);
        else return SkipVote(player, __instance);
    }

    public static byte PeacockVoting(Peacock peacock, MeetingHud __instance)
    {
        var player = peacock.Player;
        var validPlayersCrewmate = PlayerControl.AllPlayerControls.ToArray().Where(x =>
            !x.HasDied() && x != player && !x.HasModifier<TI>(x => !x.Player.HasModifier<Suspicion>()) &&
            !(x.TryGetModifier<Confirmed>(out var confirmed) && confirmed.IsConfirmed()) && !x.HasModifier<SoftCleared>() &&
            !(x.Is(Faction.Crewmate) && x.HasModifier<GlobalReveal>()) && player.GetAssociate() != x).ToList();
        var validPlayersInfiltrator = PlayerControl.AllPlayerControls.ToArray().Where(x =>
            !x.HasDied() && !x.Is(Faction.Infiltrator) && player.GetAssociate() != x).ToList();
        var validPlayersNeutral = PlayerControl.AllPlayerControls.ToArray().Where(x => !x.HasDied() && x != player && player.GetAssociate() != x).ToList();

        if (player.GetAssociate().Is(Faction.Crewmate)) return CrewmateVoting(player, __instance, validPlayersCrewmate);
        else if (player.GetAssociate().Is(Faction.Infiltrator)) return InfiltratorVoting(player, __instance, validPlayersInfiltrator);
        else return RandomVote(player, __instance, validPlayersNeutral, PlayerControl.AllPlayerControls.Count >= GlobalSkipThreshold);
    }

    public static bool ChanceIsNull(int? num)
    {
        var r = Random.Range(0, 100);
        return r < num;
    }

    public static bool ChanceIsFloat(float num)
    {
        var r = Random.Range(0f, 100f);
        return r < num;
    }

    public static bool ChanceIs(int num)
    {
        var r = Random.Range(0, 100);
        return r < num;
    }
}