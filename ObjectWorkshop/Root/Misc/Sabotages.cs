namespace ObjectWorkshop.Misc;

public static class Sabotages
{
    public static bool AnyActive()
    {
        if (OWPlugin.InGame())
        {
            var system = ShipStatus.Instance.Systems[SystemTypes.Sabotage].Cast<SabotageSystemType>();
            var specials = system.specials.ToArray();

            return specials.Any((IActivatable s) => s.IsActive);
        }

        return false;
    }
}

/*[HarmonyPatch(typeof(MapRoom), nameof(MapRoom.SabotageOxygen))]
public static class Culverin_MapRoom_SabotageOxygen
{
    public static bool Prefix(MapRoom __instance)
    {
        if (PlayerControl.LocalPlayer.IsRole<Culverin>() && __instance.Parent.CanUseSabotage)
        {
            var culverin = PlayerControl.LocalPlayer.GetRole<Culverin>();
            culverin.GainGold();
        }
        return true;
    }
}

[HarmonyPatch(typeof(MapRoom), nameof(MapRoom.SabotageReactor))]
public static class Culverin_MapRoom_SabotageReactor
{
    public static bool Prefix(MapRoom __instance)
    {
        if (PlayerControl.LocalPlayer.IsRole<Culverin>() && __instance.Parent.CanUseSabotage)
        {
            var culverin = PlayerControl.LocalPlayer.GetRole<Culverin>();
            culverin.GainGold();
        }
        return true;
    }
}

[HarmonyPatch(typeof(MapRoom), nameof(MapRoom.SabotageSeismic))]
public static class Culverin_MapRoom_SabotageSeismic
{
    public static bool Prefix(MapRoom __instance)
    {
        if (PlayerControl.LocalPlayer.IsRole<Culverin>() && __instance.Parent.CanUseSabotage)
        {
            var culverin = PlayerControl.LocalPlayer.GetRole<Culverin>();
            culverin.GainGold();
        }
        return true;
    }
}*/