using Reactor.Networking.Rpc;
using UnityEngine;

namespace ObjectWorkshop.Misc;

public static class CustomExtentions
{
    public static string GetRoleInColor(this PlayerControl player)
    {         
        if (player.GetTrueRole() is ICustomAURole customRole)
        {
            return $"<color=#{ColorUtility.ToHtmlStringRGB(customRole.RoleColor)}>{customRole.RoleName}</color>";
        }
        return player.Data.Role.NiceName;
    }

    public static string Name(this PlayerControl player)
    {
        return player.GetDefaultAppearance().PlayerName;
    }

    public static string ToSpacedString(this Enum value)
    {
        var name = value.ToString();

        // Insert space before capital letters that follow a lowercase
        name = System.Text.RegularExpressions.Regex.Replace(name, "([a-z])([A-Z])", "$1 $2");

        // Insert space when a capital is followed by another capital + lowercase (e.g., "AShroud")
        name = System.Text.RegularExpressions.Regex.Replace(name, "([A-Z])([A-Z][a-z])", "$1 $2");

        return name;
    }

    public static bool Is(this PlayerControl player, CalculatedFaction faction)
    {
        if (player == null) return false;
        if (player.HasDied())
        {
            var deadRole = player.GetRoleWhenAlive();
            if (deadRole is ICustomAURole customRole && customRole.CalculatedFaction == faction)
            {
                return true;
            }
        }

        if (player.Data.Role is ICustomAURole role && role.CalculatedFaction == faction)
        {
            return true;
        }

        return false;
    }

    public static bool Is(this PlayerControl player, Faction faction)
    {
        if (player == null) return false;
        if (player.HasDied())
        {
            var deadRole = player.GetRoleWhenAlive();
            if (deadRole is ICustomAURole customRole && customRole.Faction == faction)
            {
                return true;
            }
        }

        if (player.Data.Role is ICustomAURole role && role.Faction == faction)
        {
            return true;
        }

        return false;
    }

    public static bool Is(this PlayerControl player, Alignment alignment)
    {
        if (player.Data.Role is ICustomAURole role && role.Alignment == alignment)
        {
            return true;
        }

        return false;
    }

    public static bool AmOwner(this PlayerControl player)
    {
        return player.AmOwner || (Debugger.IsDebuggerActive && Debugger.ShowAllMessages);
    }

    public static bool IsSameFaction(this PlayerControl player, PlayerControl target)
    {
        var playerRole = player.GetICustomAURoleWhenAlive();
        var targetRole = target.GetICustomAURoleWhenAlive();

        if (playerRole.Faction == targetRole.Faction) return true;
        return false;
    }

    public static bool IsTrueRole<T>(this PlayerControl player) where T : RoleBehaviour
    {
        if (player == null) return false;
        if (player.HasDied())
        {
            var r = player.GetRoleWhenAlive();
            return r is T;
        }
        return player.Data?.Role is T;
    }

    public static RoleBehaviour? GetTrueRole(this PlayerControl player) 
    {
        if (player == null) return null;
        if (player.HasDied())
        {
            var r = player.GetRoleWhenAlive(); 
            return r;
        }

        var role = player.Data?.Role;
        return role;
    }

    public static T? GetTrueRole<T>(this PlayerControl player) where T : RoleBehaviour
    {
        if (player == null) return null;
        if (player.HasDied())
        {
            var r = player.GetRoleWhenAlive() as T;
            return r;
        }

        var role = player.Data?.Role as T;
        return role;
    }

    public static ICustomAURole GetICustomAURoleWhenAlive(this PlayerControl player)
    {
        ICustomAURole customRole = null;
        //var role = RoleHistory.LastOrDefault(x => x.Key == player.PlayerId && !x.Value.IsDead);
        //return role.Value != null ? role.Value : null;

        if (GameHistory.RoleWhenAlive.TryGetValue(player.PlayerId, out var role))
        {
            if (role is ICustomAURole c3) customRole = c3;
        }

        if (!player.Data.IsDead)
        {
            if (player.Data.Role is ICustomAURole c4) customRole = c4;
        }

        var role2 = player.Data.RoleWhenAlive;
        if (role2.HasValue)
        {
            if (RoleManager.Instance.GetRole(role2.Value) is ICustomAURole c2) customRole = c2;
        }

        if (player.Data.Role is ICustomAURole c) customRole = c;
        return customRole;
    }


    /// <summary>
    /// Networked Custom Murder method.
    /// </summary>
    /// <param name="source">The killer.</param>
    /// <param name="target">The player to murder.</param>
    [MethodRpc((uint)Rpcs.GhostRoleMurder, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcGhostRoleMurder(
        this PlayerControl source,
        PlayerControl target)
    {
        if (LobbyBehaviour.Instance)
            return;

        if (!source.HasDied())
            return;

        var role = source.GetRoleWhenAlive();
        if (source.Data.Role is IGhostRole)
        {
            role = source.Data.Role;
        }

        var customRole = role as ICustomAURole;
        if (customRole == null)
            return;

        source.CustomMurder(
            target,
            MurderResultFlags.Succeeded);

        // Force-sync death state after ghost role murder to prevent desyncs
        if (target.HasDied())
        {
            DeathStateSync.ScheduleDeathStateSync(target, true);
            // Request validation after kill to ensure all clients are in sync
            if (source.AmOwner)
            {
                DeathStateSync.RequestValidationAfterKill(source);
            }
        }
    }

    public static bool IsNeutral(this ICustomAURole customRole)
    {
        return 
            customRole.Faction == Faction.Neutral;
    }

    public static bool IsHidden(this SpriteRenderer sr)
    {
        return sr.color == new Color(1, 1, 1, 0);
    }

    public static void Hide(this SpriteRenderer sr)
    {
        sr.color = new Color(1, 1, 1, 0);
    }

    public static void Show(this SpriteRenderer sr)
    {
        sr.color = new Color(1, 1, 1, 1);
    }

    public static ShowRoleIcon GetIcon(this PlayerControl player)
    {
        foreach (var icon in ShowRoleIcon.AllRoleIcons)
        {
            if (icon.Owner == player) return icon;
        }
        return null;
    }

    public static Alignment GetAlignment(this PlayerControl player)
    {
        if (player == null) return Alignment.None;
        if (player.Data.Role is ICustomAURole customRole) return customRole.Alignment;
        return Alignment.None;
    }

    public static DeathReasonShow GetDeathReason(this PlayerControl player, DeathReasonShow newDR = DeathReasonShow.None)
    {
        if (newDR != DeathReasonShow.None) return newDR; // For roles that can apply multiple death reasons, like Toaster.
        return DeathReasonShow.Killed;
    }

    public static RoleBehaviour GetRoleFromRoleBehaviour(this RoleBehaviour role)
    {
        var ushortRole = RoleId.Get(role.GetType());
        return RoleManager.Instance.GetRole((RoleTypes)ushortRole);
    }

    public static RoleBehaviour GetRoleBehaviourFromRole<T>() where T : RoleBehaviour
    {
        var role = MiscUtils.AllRoles.Where(x => x is ICustomAURole && x is T).FirstOrDefault();
        var ushortRole = RoleId.Get(role.GetType());
        return RoleManager.Instance.GetRole((RoleTypes)ushortRole);
    }

    public static Sprite GetRoleIcon(this PlayerControl player)
    {
        if (player.Data.Role is ICustomAURole customRole)
        {
            if (customRole.Configuration.Icon != null) return customRole.Configuration.Icon.LoadAsset();
        }

        return null;
    }

    public static bool IsConfirmed(this PlayerControl player) =>
        player.HasModifier<TI>() || (player.TryGetModifier<Confirmed>(out var confirmed) && confirmed.IsConfirmed());

    public static bool CanKill(this PlayerControl player, PlayerControl target)
    {
        return true;
    }

    public static bool IsProtected(this PlayerControl target, bool bypassBasicProtection = false)
    {
        return false;
    }

    public static bool IsTargetable(this PlayerControl player)
    {
        return true;//!player.IsUnderground();
    }

    public static bool AbilityUsable(this PlayerControl player)
    {
        return
            !player.HasModifier<DuelingModifier>()// &&
            //!PeacockVisual.IsPlayerAnyParalyzed(player) &&
            //!ZapBox.IsInAnyRange(player)
            ;
    }

    public static void HideLR(this LineRenderer myRend)
    {
        myRend.startColor = new Color(0, 0, 0, 0);
        myRend.endColor = new Color(0, 0, 0, 0);
    }

    public static void ShowLR(this LineRenderer myRend, Color startColor, Color endColor)
    {
        myRend.startColor = startColor;
        myRend.endColor = endColor;
    }

    public static bool IsSpider(this PlayerControl player) => player.Data.Role is Arachnid arachnid && arachnid.isSpider;
}