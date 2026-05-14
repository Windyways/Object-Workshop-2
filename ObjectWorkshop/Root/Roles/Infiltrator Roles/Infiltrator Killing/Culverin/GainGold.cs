namespace ObjectWorkshop.Misc;

public static class GainGold
{
    [HarmonyPatch(typeof(MapRoom), nameof(MapRoom.SabotageOxygen))]
    public static class Culverin_MapRoom_SabotageOxygen
    {
        public static bool Prefix(MapRoom __instance)
        {
            if (PlayerControl.LocalPlayer.Data.Role is Culverin culverin && __instance.Parent.CanUseSabotage) culverin.GainGold();
            return true;
        }
    }

    [HarmonyPatch(typeof(MapRoom), nameof(MapRoom.SabotageReactor))]
    public static class Culverin_MapRoom_SabotageReactor
    {
        public static bool Prefix(MapRoom __instance)
        {
            if (PlayerControl.LocalPlayer.Data.Role is Culverin culverin && __instance.Parent.CanUseSabotage) culverin.GainGold();
            return true;
        }
    }

    [HarmonyPatch(typeof(MapRoom), nameof(MapRoom.SabotageSeismic))]
    public static class Culverin_MapRoom_SabotageSeismic
    {
        public static bool Prefix(MapRoom __instance)
        {
            if (PlayerControl.LocalPlayer.Data.Role is Culverin culverin && __instance.Parent.CanUseSabotage) culverin.GainGold();
            return true;
        }
    }
}