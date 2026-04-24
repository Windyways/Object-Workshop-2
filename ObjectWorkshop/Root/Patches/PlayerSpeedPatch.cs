namespace ObjectWorkshop.Patches;

[HarmonyPatch(typeof(LogicOptions), nameof(LogicOptions.GetPlayerSpeedMod))]
public static class PlayerSpeedPatch
{
    // ReSharper disable once InconsistentNaming
    public static void Postfix(PlayerControl pc, ref float __result)
    {
        __result *= pc.GetAppearance().Speed;

        /*if (pc.Data.Role is Totemist totemist && totemist.isWatching)
        {
            __result *= 0f;
        }
        else if (pc.Data.Role is Aimsman aimsman && aimsman.isAiming)
        {
            __result *= 0f;
        }
        else if (pc.Data.Role is GiftWeaver giftWeaver && giftWeaver.isBuilding)
        {
            __result *= 0f;
        }
        else if (pc.Data.Role is Culverin culverin && culverin.isAiming)
        {
            __result *= 0f;
        }
        else if (pc.IsUnderground())
        {
            __result *= OptionGroupSingleton<Excavator_Options>.Instance.Speed;
        }
        else if (PeacockVisual.IsPlayerAnyParalyzed(pc))
        {
            __result *= 0f;
        }
        else if (pc.IsRole<UndeadReaper>() && UndeadReaper.ReapersAlive())
        {
            __result *= OptionGroupSingleton<UndeadReaper_Options>.Instance.Speed;
        }
        else if (pc.HasModifier<SubmergedModifier>())
        {
            __result *= OptionGroupSingleton<Claylamity_Options>.Instance.Speed;
        }
        else */
        if (Webs.IsInWebs(pc) && pc.IsSpider())
        {
            __result *= OptionGroupSingleton<Arachnid_Options>.Instance.Speed;
        }
        else if (Webs.IsInWebs(pc))
        {
            __result *= OptionGroupSingleton<Arachnid_Options>.Instance.PreySpeed;
        }
        else if (Lightbulb.IsPlayerInAnyRange(pc))
        {
            __result *= OptionGroupSingleton<Luminescence_Options>.Instance.Speed;
        }
    }
}