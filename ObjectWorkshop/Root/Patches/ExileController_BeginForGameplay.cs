using Il2CppInterop.Runtime.InteropTypes.Arrays;
using static MeetingHud;

namespace ObjectWorkshop.Patches;

[HarmonyPatch]
public static class ExileController_BeginForGameplay
{
    // Player that almost got voted, reason via role.
    public static (PlayerControl?, RoleBehaviour?) NoLynchReason;

    [HarmonyPatch(typeof(ExileController), nameof(ExileController.BeginForGameplay))]
    [HarmonyPostfix]
    public static void ExileControllerPatch(ExileController __instance)
    {
        NetworkedPlayerInfo exiled = __instance.initData.networkedPlayer;
        PlayerControl? player = null;

        if (exiled != null)
        {
            player = exiled.Object;
            if (player.Data.Role is ICustomAURole ausRole)
            {
                string exileText = player.Name() + "'s Role Was <b><color=#" + ausRole.RoleColor.ToHtmlStringRGBA() + $">{ausRole.RoleName}</color></b>!";
                if (GameOptionsManager.Instance.currentNormalGameOptions.MapId is 7) PlayerControl.LocalPlayer.Notify(exileText, NotifyMode.Instantly);
                __instance.completeString = exileText;
            }
        }
        else
        {
            if (NoLynchReason.Item1 != null)
            {
                var lynchedPlayer = NoLynchReason.Item1;
                var role = NoLynchReason.Item2;

                string exileText = lynchedPlayer.Name() + $" cannot be lynched because they were protected.".ApplyKeywords();
                __instance.completeString = exileText;
            }
        }


        foreach (var players in PlayerControl.AllPlayerControls)
        {
            if (players.HasDied())
            {
                var role = players.GetRoleWhenAlive();
                if (role is ICustomAURole cr)
                {
                    cr.Role_OnEjection(player, __instance);
                    cr.Role_OnDeath(player);
                }
            }
            else if (players.Data.Role is ICustomAURole customRole)
            {
                customRole.Role_OnEjection(player, __instance);
                customRole.Role_OnDeath(player);
            }
        }

        NoLynchReason = (null, null);
        __instance.ImpostorText.text = "";
    }
}

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.VotingComplete))]
public static class MeetingHud_VotingComplete
{
    public static void Postfix(MeetingHud __instance, [HarmonyArgument(0)] Il2CppStructArray<VoterState> states, [HarmonyArgument(1)] NetworkedPlayerInfo exiled, [HarmonyArgument(2)] bool tie)
    {
        if (__instance.exiledPlayer != null)
        {
            var votedPlayer = MiscUtils.PlayerById(__instance.exiledPlayer.PlayerId);
            if (votedPlayer != null)
            {
                /*else if (votedPlayer.HasModifier<AdmirerProtection>())
                {
                    ExileController_BeginForGameplay.NoLynchReason = (votedPlayer, CustomExtentions.GetRoleBehaviourFromRole<Admirer>());
                    __instance.exiledPlayer = null;
                }*/
            }
        }
    }
}