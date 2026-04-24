using ObjectWorkshop.Roles;
using Reactor.Networking.Rpc;

namespace ObjectWorkshop.Mechanics;

public static class VisitingMechanic
{
    [MethodRpc((uint)Rpcs.RpcAddDeathReason)]
    public static void RpcAddDeathReason(PlayerControl player, int deathReasonShow)
    {
        DeathHandlerModifier.UpdateDeathHandler(player, (DeathReasonShow)deathReasonShow, DeathHandlerOverride.SetFalse);
    }

    public static void CheckVisit(PlayerControl player, PlayerControl target, int Button, bool isAttacking, bool isVisiting)
    {
        int blockVisit = 0;

        // --- ROLEBLOCK INTERACTIONS ---

        // --- REDIRECT INTERACTIONS ---
        if (blockVisit < 100)
        {

        }

        var playerRole = player.GetRoleWhenAlive();
        var targetRole = target?.GetRoleWhenAlive();

        ResetCooldowns(player, target, Button, isAttacking);

        // --- UNKNOWN OBSTACLE INTERACTIONS ---

        if (blockVisit < 100)
        {
            // Doesn't affect visit.
            if (isVisiting && isAttacking && target.HasModifier<Shielded>()) blockVisit += target.GetModifiers<Shielded>().Sum(x => x.PerformInteraction(player, target));
        }

        if (blockVisit > 0)
        {
            foreach (var players in PlayerControl.AllPlayerControls)
            {
                if (players.HasDied())
                {
                    var role = players.GetRoleWhenAlive();
                    if (role is ICustomAURole cr)
                    {
                        cr.Role_OnVisitFail(player, target, isAttacking, isVisiting, blockVisit);
                    }
                }
                else if (players.Data.Role is ICustomAURole customRole)
                {
                    customRole.Role_OnVisitFail(player, target, isAttacking, isVisiting, blockVisit);
                }
            }

            OWPlugin.DebugLogMessage($"{player.Name()} has failed their visit.");
            return; // Code below only runs if visit was successful.
        }

        SuccessfulVisit(player, target, Button);
    }

    public static void SuccessfulVisit(PlayerControl player, PlayerControl target, int Button)
    {
        OWPlugin.DebugLogMessage($"{player.Name()}'s visit was Successful!");

        var role = player.GetRoleWhenAlive();
        if (role is ICustomAURole customRole) customRole.Function(target, Button);
    }

    public static void ResetCooldowns(PlayerControl player, PlayerControl target, int Button, bool Attacking)
    {
        var role = player.GetRoleWhenAlive();
        var targetRole = target.GetRoleWhenAlive();

        if (player.AmOwner)
        {
            if (role is Marauder) CustomButtonSingleton<Marauder_Attack>.Instance.ResetCooldownAndOrEffect();
            if (role is Alarum) CustomButtonSingleton<Alarum_Activate>.Instance.ResetCooldownAndOrEffect();
            if (role is Obstructor)
            {
                if (Button == 1) CustomButtonSingleton<Obstructor_Attack>.Instance.ResetCooldownAndOrEffect();
                if (Button == 2) CustomButtonSingleton<Obstructor_Barricade>.Instance.ResetCooldownAndOrEffect();
            }
            if (role is Arachnid)
            {
                if (Button == 1 && !Webs.IsInWebs(target)) CustomButtonSingleton<Arachnid_Bite>.Instance.ResetCooldownAndOrEffect();
                if (Button == 2 || Button == 3)
                {
                    CustomButtonSingleton<Arachnid_Spin>.Instance.ResetCooldownAndOrEffect();
                    CustomButtonSingleton<Arachnid_InnerSpiderInfiltrator>.Instance.ResetCooldownAndOrEffect();
                }
            }
            if (role is Duelist)
            {
                if (Button == 1) CustomButtonSingleton<Duelist_Sharpen>.Instance.ResetCooldownAndOrEffect();
                if (Button == 2) CustomButtonSingleton<Duelist_Duel>.Instance.ResetCooldownAndOrEffect();
            }
            if (role is UFO)
            {
                if (Button == 1) CustomButtonSingleton<UFO_Destination>.Instance.ResetCooldownAndOrEffect();
                // if (Button == 2) CustomButtonSingleton<UFO_Abduct>.Instance.ResetCooldownAndOrEffect(); - This is handled in UFO.cs
            }
            if (role is Luminescence) CustomButtonSingleton<Luminescence_Radiate>.Instance.ResetCooldownAndOrEffect();
            if (role is Enticer) CustomButtonSingleton<Enticer_Prepare>.Instance.ResetCooldownAndOrEffect();
            if (role is Pyre) CustomButtonSingleton<Pyre_Ignite>.Instance.ResetCooldownAndOrEffect();
        }
    }
}