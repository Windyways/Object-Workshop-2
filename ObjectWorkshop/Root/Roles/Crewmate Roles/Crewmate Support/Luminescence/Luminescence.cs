using System.Text;
using UnityEngine;
using Color = UnityEngine.Color;

namespace ObjectWorkshop.Roles;

public sealed class Luminescence(IntPtr cppPtr)
    : CrewmateRole(cppPtr), ICustomAURole, IWikiDiscoverable
{
    public string RoleName { get; set; } = "Luminescence";
    public string RoleDescription => "Radiate to support the Crew!";
    public string RoleLongDescription => RoleDescription;
    public Color RoleColor { get; set; } = RoleColors.Crewmate;

    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public Faction Faction { get; set; } = Faction.Crewmate;
    public CalculatedFaction CalculatedFaction { get; set; } = CalculatedFaction.Crewmate;
    public Alignment Alignment => Alignment.CrewmateSupport;

    public string RevealText => "is scared of the dark.";
    public string Description => "Eject and dispose of all prime evildoers.";
    public string Intro => "You are a character with nyctophobia that has a sufficient supply of lightbulbs.";
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
        new($"Radiate ( {OptionGroupSingleton<Luminescence_Options>.Instance.MaxShatters} )",
            $"You can Radiate during the round.\n" +
            "You will place down a lightbulb at your position, visible to everyone.\n" +
            $"Crewmates in your radius have {OptionGroupSingleton<Luminescence_Options>.Instance.VisionCrew}x increased vision. Evils in your radius have {OptionGroupSingleton<Luminescence_Options>.Instance.VisionEvil}x decreased vision.\n" +
            "You can no longer Radiate if all your lightbulbs are Shattered.",
            OWAssets.KillSprite),

        new("Auto Radiate",
            $"You have a Brightness meter that starts at 100% and decreases {OptionGroupSingleton<Luminescence_Options>.Instance.BrightLower}% per second.\n" +
            "Once the Brightness meter reaches 0%, you will trigger your Radiate ability automatically and Shatter a lightbulb.\n" +
            $"Being in the radius of your Radiate will increase your Brightness meter by {OptionGroupSingleton<Luminescence_Options>.Instance.BrightLower}% per second.\n" +
            "Your vision is equal to your Brightness percentage. You are immune to the lights sabotage.",
            OWAssets.KillSprite),
    ];

    public void Role_OnRoundStart()
    {
        Brightness = 100;
    }

    public void Role_OnMeetingStart()
    {
        Lightbulb.CleanUp();
    }

    public void LobbyStart()
    {
        Lightbulb.CleanUp();
    }

    [MethodRpc((uint)Rpcs.RpcRadiate)]
    public static void RpcRadiate(PlayerControl player)
    {
        if (player.Data.Role is not Luminescence)
        {
            Logger<OWPlugin>.Error("RpcRadiate - Invalid Luminescence");
            return;
        }

        var luminescence = player.GetRole<Luminescence>();
        Lightbulb.Begin(player);
    }

    public void Function(PlayerControl target, int Button)
    {
        if (Button == 1) RpcRadiate(Player);
    }

    public float Brightness = 100;
}

public sealed class Luminescence_Radiate : ObjectWorkshopRoleButton<Luminescence>
{
    public override string Name => "Radiate";
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => RoleColors.Crewmate;
    public override float Cooldown => OptionGroupSingleton<Luminescence_Options>.Instance.Cooldown;
    public override LoadableAsset<Sprite> Sprite => OWAssets.KillSprite;
    public override float EffectDuration => OptionGroupSingleton<Luminescence_Options>.Instance.Duration;
    public override int MaxUses => (int)OptionGroupSingleton<Luminescence_Options>.Instance.MaxShatters;
    public override bool DecreaseCharge => false;

    protected override void OnClick() => VisitingMechanic.CheckVisit(Player, Player, 1, false, false);
}

public sealed class Luminescence_AutoRadiate : ObjectWorkshopRoleButton<Luminescence>
{
    public override string Name => "Auto Radiate";
    // No keybinds since this button is gonna be unusable.
    public override Color TextOutlineColor => Color.yellow;
    public override float Cooldown => 0.1f;
    public override LoadableAsset<Sprite> Sprite => OWAssets.KillSprite;
    public override ButtonLocation Location => ButtonLocation.BottomLeft;

    protected override void FixedUpdate(PlayerControl playerControl)
    {
        if (playerControl.Data.Role is Luminescence luminescence)
        {
            if (!MeetingHud.Instance && !ExileController.Instance && !IntroCutscene.Instance)
            {
                if (Lightbulb.IsPlayerInAnyRange(playerControl))
                {
                    if (luminescence.Brightness < 100)
                    {
                        luminescence.Brightness += (Time.deltaTime * 10);
                    }
                }
                else if (luminescence.Brightness <= 0)
                {
                    if (Player.AmOwner())
                    {
                        var button = CustomButtonSingleton<Luminescence_Radiate>.Instance;
                        if (button.UsesLeft > 0) button.DecreaseUses();

                        button.EffectActive = true;
                        button.Timer = EffectDuration;
                    }

                    luminescence.Brightness = 100;

                    Luminescence.RpcRadiate(Player);
                }
                else
                {
                    luminescence.Brightness -= (Time.deltaTime * OptionGroupSingleton<Luminescence_Options>.Instance.BrightLower);
                }
            }

            Button?.usesRemainingText.gameObject.SetActive(true);
            Button?.usesRemainingSprite.gameObject.SetActive(true);
            Button!.usesRemainingText.text = Mathf.RoundToInt(luminescence.Brightness) + "%";
        }

        base.FixedUpdate(playerControl);
    }

    protected override void OnClick()
    {
        // No function.
    }

    public override bool CanUse() // No using!
    {
        return false;
    }
}

public sealed class Luminescence_Options : AbstractOptionGroup<Luminescence>
{
    public override string GroupName => "Luminescence";

    [ModdedNumberOption("<color=#b3ffff>Luminescence</color> <color=#4a86e8>Radiate</color> Cooldown", 0f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float Cooldown { get; set; } = 25f;
    
    [ModdedNumberOption("<color=#b3ffff>Luminescence</color> <color=#4a86e8>Radiate</color> Duration", 0f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float Duration { get; set; } = 15f;

    [ModdedNumberOption("<color=#b3ffff>Luminescence</color> Max Shatters", 1f, 15f, 1f, MiraNumberSuffixes.None)]
    public float MaxShatters { get; set; } = 3f;

    [ModdedNumberOption("<color=#b3ffff>Luminescence</color> <color=#4a86e8>Radiate</color> Radius", 0.25f, 30f, 0.25f, MiraNumberSuffixes.Multiplier, "0.00")]
    public float Radius { get; set; } = 5f;

    [ModdedNumberOption("<color=#b3ffff>Luminescence</color> <color=#4a86e8>Radiate</color> Speed", 0.25f, 5f, 0.25f, MiraNumberSuffixes.Multiplier, "0.00")]
    public float Speed { get; set; } = 1.75f;

    [ModdedNumberOption("<color=#b3ffff>Luminescence</color> Brightness Decrease Multiplier", 1f, 15f, 1f, MiraNumberSuffixes.Multiplier)]
    public float BrightLower { get; set; } = 5f;

    [ModdedNumberOption("<color=#4a86e8>Radiate</color> Crewmate Vision Multiplier", 0.25f, 5f, 0.25f, MiraNumberSuffixes.Multiplier)]
    public float VisionCrew { get; set; } = 1.5f;

    [ModdedNumberOption("<color=#4a86e8>Radiate</color> Evil Vision Multiplier", 0.25f, 5f, 0.25f, MiraNumberSuffixes.Multiplier)]
    public float VisionEvil { get; set; } = 0.5f;
}