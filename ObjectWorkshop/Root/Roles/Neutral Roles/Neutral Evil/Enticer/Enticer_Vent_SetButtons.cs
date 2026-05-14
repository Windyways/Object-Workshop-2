namespace ObjectWorkshop.Roles;

[HarmonyPatch]
public static class Enticer_Vent_SetButtons
{
    [HarmonyPatch(typeof(Vent), nameof(Vent.SetButtons))]
    [HarmonyPrefix]
    public static bool EnterVent()
    {
        if (PlayerControl.LocalPlayer == null)
            return true;

        if (PlayerControl.LocalPlayer.Data == null)
            return true;

        if (PlayerControl.LocalPlayer.Data.Role is Enticer) return false;
        if (PlayerControl.LocalPlayer.Data.Role is Gravekeeper) return false;
        return true;
    }
}