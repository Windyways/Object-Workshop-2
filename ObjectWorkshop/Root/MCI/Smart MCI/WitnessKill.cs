using UnityEngine;

namespace ObjectWorkshop.MCI.SmartMCI;

public static class WitnessKill
{
    [RegisterEvent]
    public static void AfterMurderEvent(AfterMurderEvent @event)
    {
        if (MeetingHud.Instance || !Debugger.IsDebuggerActive || !Debugger.SmartBotsEnabled)
            return;

        var killer = @event.Source;
        var target = @event.Target;
        TryWitness(killer, target);
    }

    [RegisterEvent]
    public static void EnterVentEvent(EnterVentEvent @event)
    {
        if (MeetingHud.Instance || !Debugger.IsDebuggerActive || !Debugger.SmartBotsEnabled)
            return;

        var venter = @event.Player;
        TryWitness(venter, venter);
    }

    [RegisterEvent]
    public static void ExitVentEvent(ExitVentEvent @event)
    {
        if (MeetingHud.Instance || !Debugger.IsDebuggerActive || !Debugger.SmartBotsEnabled)
            return;

        var venter = @event.Player;
        TryWitness(venter, venter);
    }

    public static void TryWitness(PlayerControl killer, PlayerControl target)
    {
        if (killer.HasModifier<InvisibleStatus>())
            return;

        if (Vector2.Distance(killer.GetTruePosition(), target.GetTruePosition()) >= 0.75f)
            return;

        foreach (var witness in PlayerControl.AllPlayerControls)
        {
            if (witness != target && witness != killer && !witness.HasDied())
            {
                if (!IgnoreKill(killer, witness) && BotCanSee(killer, witness, target.transform.position) && witness != target)
                {
                    OWPlugin.DebugLogMessage($"{witness.Name()} has witnessed {killer.Name()} do something bad!");

                    // Add murder see modifier here.
                    var modifier = witness.AddModifier<SeenKill>();
                    if (modifier != null) modifier.killer = killer;

                    var modifier2 = killer.AddModifier<SeenKill>();
                    if (modifier2 != null) modifier2.killer = witness;
                }
            }
        }
    }

    public static bool IgnoreKill(PlayerControl killer, PlayerControl witness)
    {
        if (killer.IsRole<Duelist>()) return true; // this duel stuff is PMO.
        if (killer.IsSpider() || witness.HasModifier<DuelingModifier>() || killer.HasModifier<DuelingModifier>()) return true;
        if (killer.Is(Faction.Infiltrator) && witness.Is(Faction.Infiltrator)) return true;
        return false;
    }

    public static bool BotCanSee(PlayerControl killer, PlayerControl witness, Vector3 killPos, bool ignoreInvis = false)
    {
        // 1) distance
        float dist = Vector3.Distance(witness.transform.position, killPos);
        float baseVision = GameOptionsManager.Instance.currentNormalGameOptions.CrewLightMod * 2;

        // impostor bots maybe get impostor mod — you decide:
        if (witness.Is(Faction.Infiltrator)) baseVision = GameOptionsManager.Instance.currentNormalGameOptions.ImpostorLightMod / 2;
        //else if (witness.Data.Role is ICustomAURole customRole && witness.Is(Faction.Neutral)) baseVision = customRole.visionValue;

        // scale vision by map lighting (lights sabotage etc.)
        float vision = baseVision * ShipStatus.Instance.CalculateLightRadius(witness.Data);

        if (dist > vision)
            return false;

        var vector = witness.GetTruePosition() - killer.GetTruePosition();
        var magnitude = vector.magnitude;

        if (PhysicsHelpers.AnyNonTriggersBetween(killer.GetTruePosition(), vector.normalized, magnitude, Constants.ShipAndObjectsMask))
            return false;
        
        if (killer.HasModifier<InvisibleTogglable>() && !ignoreInvis)
            return false;

        return true;
    }

    public static void WitnessEvilDeed(PlayerControl player)
    {
        if (Debugger.IsDebuggerActive && Debugger.SmartBotsEnabled)
        {
            foreach (var witness in PlayerControl.AllPlayerControls)
            {
                if (!IgnoreKill(player, witness) && !player.IsRole<Shikari>() && BotCanSee(player, witness, player.transform.position))
                {
                    var modifier = witness.AddModifier<SeenKill>();
                    if (modifier != null) modifier.killer = player;

                    var modifier2 = player.AddModifier<SeenKill>();
                    if (modifier2 != null) modifier2.killer = witness;
                }
            }
        }
    }

    public static void WitnessGoodDeed(PlayerControl player)
    {
        if (Debugger.IsDebuggerActive && Debugger.SmartBotsEnabled)
        {
            foreach (var witness in PlayerControl.AllPlayerControls)
            {
                if (BotCanSee(player, witness, player.transform.position))
                {
                    player.AddModifier<Confirmed>(witness, ConfirmType.Instantly, player.Data.Role.NiceName);
                }
            }
        }
    }
}