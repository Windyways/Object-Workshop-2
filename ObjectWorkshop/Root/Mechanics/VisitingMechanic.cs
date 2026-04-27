using Random = UnityEngine.Random;

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

        var playerRole = player.GetRoleWhenAlive();
        var targetRole = target?.GetRoleWhenAlive();

        // --- ROLEBLOCK INTERACTIONS ---
        foreach (var sandstorm in Sandstorm.AllSandstorms)
        {
            var num = Random.Range(0, 100);
            if (num <= OptionGroupSingleton<Oasis_Options>.Instance.MissChance && isAttacking && isVisiting)
            {
                Oasis.RpcSandstormMissNotif(player, target, sandstorm.Owner);
                blockVisit += 100;
            }
        }

        // --- REDIRECT INTERACTIONS ---
        if (blockVisit < 100)
        {
            if (targetRole is BookCollector bookCollector && bookCollector.GuessedPlayers.Count > 0 && isVisiting && isAttacking)
            {
                var interaction = bookCollector.PerformInteraction(player);
                if (interaction.Item1 != null)
                {
                    blockVisit = interaction.Item2;
                    target = interaction.Item1;
                }
            }

            if (Sanctuary.IsPlayerInAnyRange(target, out var Owner) && isAttacking && isVisiting)
            {
                blockVisit += 1;
                Oasis.RpcSanctifyNotif(player, target, Owner);
            }
            else if (isVisiting && isAttacking && target.HasModifier<Shielded>()) blockVisit += target.GetModifiers<Shielded>().Sum(x => x.PerformInteraction(player, target));
        }

        ResetCooldowns(player, target, Button, isAttacking);

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

    public static void ResetCooldowns(PlayerControl player, PlayerControl? target, int Button, bool Attacking)
    {
        var role = player.GetRoleWhenAlive();
        var targetRole = target?.GetRoleWhenAlive();

        if (player.AmOwner)
        {
            if (role is Marauder) CustomButtonSingleton<Marauder_Attack>.Instance.ResetCooldownAndOrEffect();
            if (role is Alarum) CustomButtonSingleton<Alarum_Activate>.Instance.ResetCooldownAndOrEffect();
            if (role is Obstructor)
            {
                if (Button == 1) CustomButtonSingleton<Obstructor_Attack>.Instance.ResetCooldownAndOrEffect();
                if (Button == 2) CustomButtonSingleton<Obstructor_Barricade>.Instance.ResetCooldownAndOrEffect();
            }
            if (role is Arachnid arachnid)
            {
                if (Button == 1 && !Webs.IsInWebs(target)) CustomButtonSingleton<Arachnid_Bite>.Instance.ResetCooldownAndOrEffect();
                if (Button == 2 || Button == 3)
                {
                    CustomButtonSingleton<Arachnid_Spin>.Instance.ResetCooldownAndOrEffect();
                    if (arachnid.isSpider || Button == 2) CustomButtonSingleton<Arachnid_InnerSpiderInfiltrator>.Instance.ResetCooldownAndOrEffect();
                }
            }
            if (role is Duelist)
            {
                if (Button == 1) CustomButtonSingleton<Duelist_Sharpen>.Instance.ResetCooldownAndOrEffect();
                // if (Button == 2) CustomButtonSingleton<Duelist_Duel>.Instance.ResetCooldownAndOrEffect(); - This is handled in DuelController.cs
            }
            if (role is UFO)
            {
                if (Button == 1) CustomButtonSingleton<UFO_Destination>.Instance.ResetCooldownAndOrEffect();
                // if (Button == 2) CustomButtonSingleton<UFO_Abduct>.Instance.ResetCooldownAndOrEffect(); - This is handled in UFO.cs
            }
            if (role is Luminescence) CustomButtonSingleton<Luminescence_Radiate>.Instance.ResetCooldownAndOrEffect();
            if (role is Enticer) CustomButtonSingleton<Enticer_Prepare>.Instance.ResetCooldownAndOrEffect();
            if (role is Pyre) CustomButtonSingleton<Pyre_Ignite>.Instance.ResetCooldownAndOrEffect();
            if (role is Claylamity)
            {
                if (Button == 1) CustomButtonSingleton<Claylamity_Attack>.Instance.ResetCooldownAndOrEffect();
                if (Button == 2)
                {
                    CustomButtonSingleton<Claylamity_Destination>.Instance.ResetCooldownAndOrEffect();
                    // CustomButtonSingleton<Claylamity_Duel>.Instance.ResetCooldownAndOrEffect(); - This is handled in DuelController.cs
                    CustomButtonSingleton<Claylamity_Radiate>.Instance.ResetCooldownAndOrEffect();
                }
                if (Button == 3) CustomButtonSingleton<Claylamity_Metamorphosis>.Instance.ResetCooldownAndOrEffect();
            }
            if (role is Aimsman)
            {
                if (Button == 1) CustomButtonSingleton<Aimsman_Aim>.Instance.ResetCooldownAndOrEffect();
                if (Button == 2) CustomButtonSingleton<Aimsman_Fire>.Instance.ResetCooldownAndOrEffect();
            }
            if (role is BookCollector) CustomButtonSingleton<BookCollector_Read>.Instance.ResetCooldownAndOrEffect();
            if (role is Peacock peacock)
            {
                if (Button == 1) CustomButtonSingleton<Peacock_Declare>.Instance.ResetCooldownAndOrEffect();
                if (Button == 2 && peacock.isBloomed) CustomButtonSingleton<Peacock_Bloom>.Instance.ResetCooldownAndOrEffect();
            }
            if (role is Oasis)
            {
                if (Button == 1) CustomButtonSingleton<Oasis_Sanctify>.Instance.ResetCooldownAndOrEffect();
                if (Button == 2) CustomButtonSingleton<Oasis_Sandstorm>.Instance.ResetCooldownAndOrEffect();
            }
        }
    }
}