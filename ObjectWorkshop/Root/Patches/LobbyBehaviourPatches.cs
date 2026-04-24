namespace ObjectWorkshop.Patches;

[HarmonyPatch]
public static class LobbyBehaviourPatches
{
    [HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.Start))]
    [HarmonyPostfix]
    public static void LobbyStartPatch(LobbyBehaviour __instance)
    {
        foreach (var role in GameHistory.AllRoles)
        {
            if (!role || role is not ICustomAURole touRole)
            {
                continue;
            }

            touRole.LobbyStart();
        }

        GameHistory.ClearAll();
        ScreenFlash.Clear();
        MeetingMenu.ClearAll();

        ShowRoleIcon.ClearAll();

        ExileController_BeginForGameplay.NoLynchReason = (null, null);

        // --- MECHANICS ---
        DayNightMechanic.DayCount = 0;
        DayNightMechanic.NightCount = 1;
        HuntMechanic.StopHunt();

        RolelistMechanic.InfiltratorCount = 0;
        InfiltratorGameOver.SabotageWin = false;

        if (RoleReferences.PendingNotifications.Count != 0)
        {
            foreach (var msg in RoleReferences.PendingNotifications)
            {
                MiscUtils.AddFakeChat(PlayerControl.LocalPlayer.CachedPlayerData, "PLACEMENT CHANGES", msg);
            }

            RoleReferences.PendingNotifications.Clear(); // prevent repeats
        }

        // NO DLEKS!!!
        if (GameOptionsManager.Instance.currentNormalGameOptions.MapId is 3)
        {
            GameOptionsManager.Instance.currentNormalGameOptions.MapId = 0;
        }
    }
}