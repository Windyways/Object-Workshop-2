using UnityEngine;

namespace TownOfUs.Utilities;

public static class PlayerRoleTextExtensions
{
    public static Color UpdateTargetColor(this Color color, PlayerControl player, bool hidden = false)
    {
        /*if (player.TryGetModifier<BodyguardProtect>(out var bodyguardProtect) && bodyguardProtect.Caster == PlayerControl.LocalPlayer)
        {
            color = RoleColors.Bodyguard;
        }*/

        return color;
    }

    public static string UpdateStatusSymbols(this string name, PlayerControl player, bool hidden = false)
    {
        //var genOpt = OptionGroupSingleton<GeneralOptions>.Instance;

        /*if (player.TryGetModifier<AdmirerTarget>(out var admirerTarget) && admirerTarget.Caster == PlayerControl.LocalPlayer)
        {
            name += $"<color=#{RoleColors.Admirer.ToHtmlStringRGBA()}> !</color>";
        }*/
        return name;
    }
}


/* Status Alphabet

Ⓐ
Ⓑ
Ⓒ
Ⓓ
Ⓔ
Ⓕ
Ⓖ
Ⓗ
Ⓘ
Ⓙ
Ⓚ
Ⓛ
Ⓜ
Ⓝ
Ⓞ
Ⓟ
Ⓠ
Ⓡ
Ⓢ
Ⓣ
Ⓤ
Ⓥ
Ⓦ
Ⓧ
Ⓨ
Ⓩ

*/