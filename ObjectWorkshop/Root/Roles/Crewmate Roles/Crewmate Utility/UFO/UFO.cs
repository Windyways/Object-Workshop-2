using System.Text;
using UnityEngine;
using Color = UnityEngine.Color;
using Object = UnityEngine.Object;

namespace ObjectWorkshop.Roles;

public sealed class UFO(IntPtr cppPtr) 
    : CrewmateRole(cppPtr), ICustomAURole, IWikiDiscoverable
{
    public string RoleName { get; set; } = "UFO";
    public string RoleDescription => "Abduct players to the Moon!";
    public string RoleLongDescription => RoleDescription;
    public Color RoleColor { get; set; } = RoleColors.Crewmate;

    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public Faction Faction { get; set; } = Faction.Crewmate;
    public CalculatedFaction CalculatedFaction { get; set; } = CalculatedFaction.Crewmate;
    public Alignment Alignment => Alignment.CrewmateUtility;

    public string RevealText => "owns a mythological ship.";
    public string Description => "Assign a Destination for the moon, then Abduct players to the moon.";
    public string Intro => "You are a foreign astronaut that believes in universal lifeforms.";
    public string VictoryCondition => "Dispose of all evil. You will win with other Crewmate members.";

    public CustomRoleConfiguration Configuration => new(this)
    {

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
        new("Destination",
            "You can set a Destination during the round.\n" +
            "You will place down the moon, visible to everyone. Using this ability again overrides any existing moons.\n" +
            "Obstructor's Barriers can destroy your moon.",
            OWAssets.KillSprite),

        new("Abduct",
            $"You can Abduct a player during the round.\n" +
            $"You will select your target via a menu, after {OptionGroupSingleton<UFO_Options>.Instance.Duration}s, your target will be teleported onto the moon you placed beforehand.\n" +
            $"You may abduct dead bodies.\n" +
            "You may not Abduct without a moon.",
            OWAssets.KillSprite)
    ];

    [MethodRpc((uint)Rpcs.RpcDestination)]
    public static void RpcDestination(PlayerControl player)
    {
        if (player.Data.Role is not UFO)
        {
            Logger<OWPlugin>.Error("PlaceMoon - Invalid UFO");
            return;
        }

		Moon.DestroyAll();
		Moon.Begin(player);
    }
    
    [MethodRpc((uint)Rpcs.RpcAbduct)]
    public static void RpcAbduct(PlayerControl player, PlayerControl target)
    {
        if (player.Data.Role is not UFO)
        {
            Logger<OWPlugin>.Error("RpcAbduct - Invalid UFO");
            return;
        }

        var ufo = player.GetRole<UFO>();
        if (ufo != null)
        {
            var moon = Moon.GetMoon();
            Coroutines.Start(moon.PlaySwirlAndTeleport(ufo.Player, target));
        }
    }

    public void LobbyStart()
    {
        Moon.CleanUp();
    }

    public static string Info(PlayerControl target)
    {
        return $"You tried to Abduct {target.Name()}, but they were immune!";
    }

    public void Function(PlayerControl target, int Button)
    {
        if (Button == 1) RpcDestination(Player);
        else if (Button == 2)
        {
            var playerMenu = CustomPlayerMenu.Create();
            playerMenu.transform.FindChild("PhoneUI").GetChild(0).GetComponent<SpriteRenderer>().material = PlayerControl.LocalPlayer.cosmetics.currentBodySprite.BodySprite.material;
            playerMenu.transform.FindChild("PhoneUI").GetChild(1).GetComponent<SpriteRenderer>().material = PlayerControl.LocalPlayer.cosmetics.currentBodySprite.BodySprite.material;
            playerMenu.Begin(
                plr => !plr.HasDied() ||
                        Object.FindObjectsOfType<DeadBody>().FirstOrDefault(x => x.ParentId == plr.PlayerId) ||
                        FakePlayer.FakePlayers.FirstOrDefault(x => x?.body?.name == $"Fake {plr.gameObject.name}")?.body, plr =>
                        {
                            playerMenu.ForceClose();

                            if (plr != null)
                            {
                                OWPlugin.DebugLogMessage("UFO is Abducting...");
                                RpcAbduct(Player, plr);

                                if (Player.AmOwner)
                                {
                                    var abductButton = CustomButtonSingleton<UFO_Abduct>.Instance;
                                    abductButton.EffectActive = true;
                                    abductButton.Timer = abductButton.EffectDuration;
                                }
                            }
                        });

            foreach (var panel in playerMenu.potentialVictims)
            {
                panel.PlayerIcon.cosmetics.SetPhantomRoleAlpha(1f);
                if (panel.NameText.text != PlayerControl.LocalPlayer.Data.PlayerName)
                {
                    panel.NameText.color = Color.white;
                }
            }
        }
    }
}

public sealed class UFO_Destination : ObjectWorkshopRoleButton<UFO>
{
    public override string Name => "Destination";
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => RoleColors.Crewmate;
    public override float Cooldown => OptionGroupSingleton<UFO_Options>.Instance.Cooldown;
    public override LoadableAsset<Sprite> Sprite => OWAssets.KillSprite;

    protected override void OnClick() => VisitingMechanic.CheckVisit(Player, Player, 1, false, false);
    public override bool CanUse()
    {
        var moon = Moon.GetMoon();
        if (moon == null) return base.CanUse();
        return base.CanUse() && !Moon.GetMoon().isAbductOccuring;
    }
}

public sealed class UFO_Abduct : ObjectWorkshopRoleButton<UFO>
{
    public override string Name => "Abduct";
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => RoleColors.Crewmate;
    public override float Cooldown => OptionGroupSingleton<UFO_Options>.Instance.AbductCD;
    public override LoadableAsset<Sprite> Sprite => OWAssets.KillSprite;
    public override float EffectDuration => OptionGroupSingleton<UFO_Options>.Instance.Duration;

    protected override void OnClick() => VisitingMechanic.CheckVisit(Player, Player, 2, false, false);
    public override void ClickHandler()
    {
        if (!CanClick())
            return;

        if (LimitedUses)
        {
            UsesLeft--;
            Button?.SetUsesRemaining(UsesLeft);

            if (TextOutlineColor != Color.clear)
            {
                SetTextOutline(TextOutlineColor);
                if (Button != null)
                {
                    Button.usesRemainingSprite.color = TextOutlineColor;
                }
            }


        }

        OnClick();
    }

    public override bool CanUse()
    {
        var moon = Moon.GetMoon();
        if (moon == null) return false;
        return base.CanUse() && !Moon.GetMoon().isAbductOccuring;
    }
}
public sealed class UFO_Options : AbstractOptionGroup<UFO>
{
    public override string GroupName => "UFO";

    [ModdedNumberOption("<color=#b3ffff>UFO</color> <color=#4a86e8>Destination</color> Cooldown", 0f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float Cooldown { get; set; } = 25f;

    [ModdedNumberOption("<color=#b3ffff>UFO</color> <color=#4a86e8>Abduct</color> Cooldown", 0f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float AbductCD { get; set; } = 25f;

    [ModdedNumberOption("<color=#b3ffff>UFO</color> <color=#4a86e8>Abduct</color> Duration", 0.25f, 60f, 0.05f, MiraNumberSuffixes.Seconds, "0.00")]
    public float Duration { get; set; } = 1.5f;
}