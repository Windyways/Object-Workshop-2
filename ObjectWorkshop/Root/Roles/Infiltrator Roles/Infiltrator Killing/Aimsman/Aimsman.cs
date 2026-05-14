using System.Text;
using UnityEngine;

namespace ObjectWorkshop.Roles;

public sealed class Aimsman(IntPtr cppPtr)
    : NeutralRole(cppPtr), ICustomAURole, IWikiDiscoverable
{
    public string RoleName { get; set; } = "Aimsman";
    public string RoleDescription => "Shoot players down remotely.";
    public string RoleLongDescription => RoleDescription;
    public Color RoleColor { get; set; } = RoleColors.Infiltrator;

    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public Faction Faction { get; set; } = Faction.Infiltrator;
    public CalculatedFaction CalculatedFaction { get; set; } = CalculatedFaction.Infiltrator;
    public Alignment Alignment => Alignment.InfiltratorKilling;

    public string RevealText => "is a corrupt sniper.";
    public string Description => "Enter Aim mode and Shoot players remotely.";
    public string Intro => "You are a corrupt global sniper that shoots people from afar.";
    public string VictoryCondition => "Kill everyone that opposes the Infiltrators. You will win with other Infiltrator members.";

    public CustomRoleConfiguration Configuration => new(this)
    {
        CanUseSabotage = OptionGroupSingleton<InfiltratorOptions>.Instance.CanSabotage,
        CanUseVent = OptionGroupSingleton<InfiltratorOptions>.Instance.CanVent,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>(),
        Icon = OWAssets.Aimsman
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ICustomAURole.SetNewTabText(this);
    }

    public string GetAdvancedDescription()
    {
        return
            $"{Intro}\n\n{Description}\n\n" +
            $"<color=#00ff00>Victory Condition:</color>\n" +
            $"{VictoryCondition}" +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new("Aim",
            "You can begin an Aim during the round.\n" +
            "You will enter Target Mode, visually showing a target to all players, which starts at the middle of your screen.\n" +
            "Using this ability again while in Target Mode will turn you back to normal.",
            OWAssets.KillSprite),

        new("Fire",
            $"You can Fire at a target during the round.\n" +
            "The player within your target will die, and you will exit Target Mode 1 second later.\n" +
            "You may shoot people in vents.",
            OWAssets.KillSprite),
    ];

    public bool WinConditionMet() => InfiltratorGameOver.WinConditionMet();
    public override bool DidWin(GameOverReason gameOverReason)
    {
        return WinConditionMet() || InfiltratorGameOver.AnyWon(gameOverReason);
    }

    public void Role_OnMeetingStart()
    {
		Crosshair.DestroyAll();
    }

    public void Role_OnDeath(PlayerControl? player)
    {
        if (player == Player && isAiming)
        {
            var crosshair = Crosshair.GetObjectByPlayer(Player);
            crosshair.DestroyGameObject();
            isAiming = false;
		}
    }

    public void LobbyStart()
    {
        Crosshair.CleanUp();
    }

    [MethodRpc((uint)Rpcs.RpcFire)]
    public static void RpcFire(PlayerControl player)
    {
        if (player.Data.Role is not Aimsman)
        {
            Logger<OWPlugin>.Error("RpcFire - Invalid Aimsman");
            return;
        }

        if (player.GetTrueRole() is Aimsman aimsman && aimsman.isAiming)
        {
            var crosshair = Crosshair.GetObjectByPlayer(aimsman.Player);
            crosshair.TryShoot(aimsman, null);
        }
    }

    [MethodRpc((uint)Rpcs.RpcAim)]
    public static void RpcAim(PlayerControl player)
    {
        if (player.Data.Role is not Aimsman)
        {
            Logger<OWPlugin>.Error("RpcAim - Invalid Aimsman");
            return;
        }

        if (player.GetTrueRole() is Aimsman aimsman)
        {
            if (!aimsman.isAiming)
            {
                aimsman.isAiming = true;
                Crosshair.Begin(aimsman.Player);
            }
            else
            {
                var crosshair = Crosshair.GetObjectByPlayer(player);
                crosshair.DestroyGameObject();
                aimsman.isAiming = false;

                if (player.AmOwner)
                {
                    var button = CustomButtonSingleton<Aimsman_Aim>.Instance;
                    button.EffectActive = false;
                    button.Timer = OptionGroupSingleton<Aimsman_Options>.Instance.Cooldown;
                }
            }
        }
    }

    [MethodRpc((uint)Rpcs.RpcMoveCrosshair)]
    public static void RpcMoveCrosshair(PlayerControl player, float x, float y)
    {
        if (player.Data.Role is not Aimsman)
        {
            Logger<OWPlugin>.Error("RpcMoveCrosshair - Invalid Aimsman");
            return;
        }

        var crosshair = Crosshair.GetObjectByPlayer(player);
        //crosshair.gameObject.transform.position = new Vector2(x, y);
        //crosshair.gameObject.transform.position = Vector2.Lerp(crosshair.gameObject.transform.position, new Vector2(x, y), crosshair.moveSpeed * Time.deltaTime);
        crosshair.gameObject.transform.position = Vector2.Lerp(crosshair.gameObject.transform.position, new Vector2(x, y), 0.3f);
    }

    [MethodRpc((uint)Rpcs.RpcDestroyCrosshair)]
    public static void RpcDestroyCrosshair(PlayerControl player)
    {
        if (player.Data.Role is not Aimsman)
        {
            Logger<OWPlugin>.Error("RpcDestroyCrosshair - Invalid Aimsman");
            return;
        }

        var crosshair = Crosshair.GetObjectByPlayer(player);
        crosshair.DestroyGameObject();
    }

    public void Function(PlayerControl target, int Button)
    {
        if (Button == 1) RpcAim(Player);
        else if (Button == 2) RpcFire(Player);
    }

    public bool isAiming;
}

public sealed class Aimsman_Aim : ObjectWorkshopRoleButton<Aimsman>
{
    public override string Name => "Aim";
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => RoleColors.Infiltrator;
    public override float Cooldown => OptionGroupSingleton<Aimsman_Options>.Instance.Cooldown;
    public override LoadableAsset<Sprite> Sprite => OWAssets.KillSprite;
    public override float EffectDuration => OptionGroupSingleton<Aimsman_Options>.Instance.Duration;

    protected override void OnClick() => VisitingMechanic.CheckVisit(Player, Player, 1, false, false);
    public override bool CanClick()
    {
        return (Timer <= 0 && CanUse()) || EffectActive;
    }

    public override void OnEffectEnd()
    {
        var crosshair = Crosshair.GetObjectByPlayer(Player);
        crosshair.DestroyGameObject();
        Role.isAiming = false;
    }
}

public sealed class Aimsman_Fire : ObjectWorkshopRoleButton<Aimsman>
{
    public override string Name => "Fire";
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => RoleColors.Infiltrator;
    public override float Cooldown => OptionGroupSingleton<Aimsman_Options>.Instance.Cooldown;
    public override LoadableAsset<Sprite> Sprite => OWAssets.KillSprite;

    protected override void OnClick() => VisitingMechanic.CheckVisit(Player, Player, 2, false, false);
    public override bool CanUse()
    {
        var crosshair = Crosshair.GetObjectByPlayer(Player);
        if (crosshair == null) return false;
        return base.CanUse() && crosshair.currentTarget != null && Role.isAiming;
    }
}

public sealed class Aimsman_Options : AbstractOptionGroup<Aimsman>
{
    public override string GroupName => "Aimsman";

    [ModdedNumberOption("<color=#ff5050>Aimsman</color> <color=#4a86e8>Aim</color> Cooldown", 0f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float Cooldown { get; set; } = 25f;

    [ModdedNumberOption("<color=#ff5050>Aimsman</color> <color=#4a86e8>Aim</color> Duration", 0.5f, 60f, 0.5f, MiraNumberSuffixes.Seconds, "0.0")]
    public float Duration { get; set; } = 30f;

    [ModdedToggleOption("Players On Ladder Are Immune")]
    public bool LadderImmunity { get; set; } = true;
}