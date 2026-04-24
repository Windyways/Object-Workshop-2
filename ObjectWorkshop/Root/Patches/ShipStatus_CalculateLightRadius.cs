using UnityEngine;

namespace ObjectWorkshop.Patches;

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CalculateLightRadius))]
public static class ShipStatus_CalculateLightRadius
{
    public static void Postfix(ShipStatus __instance, NetworkedPlayerInfo player, ref float __result)
    {
        if (player == null || player.IsDead)
        {
            __result = __instance.MaxLightRadius;
            return;
        }

        float visionValue = GameOptionsManager.Instance.currentNormalGameOptions.CrewLightMod;
        var visionFactor = 1f;

        var playerControl = player._object;

        /*if (playerControl.IsUnderground())
        {
            __result = __instance.MaxLightRadius * 0f;
            return;
        }

        if (DarkCloud.IsPlayerInAnyRange(playerControl) && !playerControl.IsRole<Canopy>())
        {
            __result = __instance.MaxLightRadius * 0f;
            return;
        }*/

        if (EnticerHook.IsHooked(playerControl))
        {
            var hook = EnticerHook.GetAll().Where(x => x.currentTarget == playerControl).ToList();
            __result = __instance.MaxLightRadius * (hook.FirstOrDefault()?.DevourVision ?? 0f);
            return;
        }

        if (Lightbulb.IsPlayerInAnyRange(playerControl))
        {
            visionFactor = playerControl.Is(Faction.Crewmate) ?
                OptionGroupSingleton<Luminescence_Options>.Instance.VisionCrew : OptionGroupSingleton<Luminescence_Options>.Instance.VisionEvil;
        }

        if (playerControl.HasModifier<DuelingModifier>())
        {
            __result = __instance.MaxLightRadius * 0.25f;
        }
        else if (playerControl.HasModifier<IgnitedModifier>())
        {
            __result = __instance.MaxLightRadius * 1f;
        }
        else if (playerControl.Data.Role is Luminescence luminescence)
        {
            __result = __instance.MaxLightRadius * (luminescence.Brightness / 33);
        }
        else if (playerControl.Is(Faction.Infiltrator))
        {
            __result = __instance.MaxLightRadius * GameOptionsManager.Instance.currentNormalGameOptions.ImpostorLightMod * visionFactor;
        }
        else if (playerControl.Data.Role is NeutralRole neutral)
        {
            __result = __instance.MaxLightRadius * neutral.Vision() * visionFactor;
        }
        else if (ModCompatibility.IsSubmerged())
        {
            __result *= visionFactor;
        }
        else
        {
            SwitchSystem? switchSystem = null;
            if (__instance.Systems != null && __instance.Systems.TryGetValue(SystemTypes.Electrical, out var system))
            {
                switchSystem = system.TryCast<SwitchSystem>();
            }

            var t = switchSystem?.Level ?? 1;

            __result = Mathf.Lerp(__instance.MinLightRadius, __instance.MaxLightRadius, t) *
                       visionValue * visionFactor;
        }
    }
}