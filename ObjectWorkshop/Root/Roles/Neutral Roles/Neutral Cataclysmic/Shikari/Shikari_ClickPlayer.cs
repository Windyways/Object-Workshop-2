namespace ObjectWorkshop.Roles;

[HarmonyPatch]
public static class Shikari_ClickPlayer
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.OnClick))]
    [HarmonyPrefix]
    public static void GhostRoleClickPatch(PlayerControl __instance)
    {
        if (MeetingHud.Instance)
            return;

        if (PlayerControl.LocalPlayer.HasDied() || __instance.HasDied())
            return;

        if (PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.Data == null)
            return;

        if (__instance.IsHost())
        {
            var host = MiscUtils.PlayerById(GameData.Instance.GetHost().PlayerId);
            var nearHost = !PhysicsHelpers.AnythingBetween(PlayerControl.LocalPlayer.GetTruePosition(), host.GetTruePosition(), Constants.ShipAndObjectsMask, false);
            if (PlayerControl.LocalPlayer.Data.Role is Shikari shikari2 && nearHost && host != shikari2.Player)
            {
                if (shikari2.ExecutionPhase()) shikari2.Execute(host);
                else if (CustomButtonSingleton<Shikari_Mark>.Instance.Timer <= 0 && !shikari2.MarkedPlayers.Contains(host)) shikari2.Mark(host);
            }

            return;
        }

        var nearPlayer = !PhysicsHelpers.AnythingBetween(PlayerControl.LocalPlayer.GetTruePosition(), __instance.GetTruePosition(), Constants.ShipAndObjectsMask, false);
        if (PlayerControl.LocalPlayer.Data.Role is Shikari shikari && nearPlayer && __instance != shikari.Player)
        {
            if (shikari.ExecutionPhase()) shikari.Execute(__instance);
            else if (CustomButtonSingleton<Shikari_Mark>.Instance.Timer <= 0 && !shikari.MarkedPlayers.Contains(__instance)) shikari.Mark(__instance);
        }
    }
}