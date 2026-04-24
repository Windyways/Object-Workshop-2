using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ObjectWorkshop.Roles;

public sealed class Duelist(IntPtr cppPtr)
    : CrewmateRole(cppPtr), ICustomAURole, IWikiDiscoverable
{
    public string RoleName { get; set; } = "Duelist";
    public string RoleDescription => "Sharpen to increase your chance to win Duels!";
    public string RoleLongDescription => RoleDescription;
    public Color RoleColor { get; set; } = RoleColors.Crewmate;

    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public Faction Faction { get; set; } = Faction.Crewmate;
    public CalculatedFaction CalculatedFaction { get; set; } = CalculatedFaction.Crewmate;
    public Alignment Alignment => Alignment.CrewmateKilling;

    public string RevealText => "is a samurai in training.";
    public string Description => "Duel players to kill them with a chance, although failing will kill you. Sharpen your sword to increase your win chance.";
    public string Intro => "You are a trained samurai that’s willing to help the crew to rid of evils.";
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
        new("Sharpen",
            $"You can Sharpen your sword during the round.\n" +
            $"You will increase your Win Chance by {OptionGroupSingleton<Duelist_Options>.Instance.ChanceUp}%, up to 100%.\n" +
            $"Your Win Chance will start off at {OptionGroupSingleton<Duelist_Options>.Instance.InitialChance}%, and cannot be lower.",
            OWAssets.KillSprite),

        new("Duel",
            "You can Duel a player during the round.\n" +
            "You will both be immobile and unable to use any abilities. You and your target will dash towards each other. You will kill your target based on your Win Chance. Your Win Chance will reset afterwards.\n" +
            "If you lose, your target will kill you.\n" +
            "If you or your target die before the duel ends, the duel will be canceled, your Duel cooldown will reset, and your Win Chance will remain.",
            OWAssets.KillSprite)
    ];

    [MethodRpc((uint)Rpcs.RpcResetWinChance)]
    public static void RpcResetWinChance(PlayerControl player)
    {
        if (player.Data.Role is not Duelist)
        {
            Logger<OWPlugin>.Error("RpcResetWinChance - Invalid Duelist");
            return;
        }

        var duelist = player.GetRole<Duelist>();
        duelist.WinChance = (int)OptionGroupSingleton<Duelist_Options>.Instance.InitialChance;
    }

    [MethodRpc((uint)Rpcs.RpcSharpen)]
    public static void RpcSharpen(PlayerControl player)
    {
        if (player.Data.Role is not Duelist)
        {
            Logger<OWPlugin>.Error("RpcSharpen - Invalid duelist");
            return;
        }

        var duelist = player.GetRole<Duelist>();
        duelist.WinChance += (int)OptionGroupSingleton<Duelist_Options>.Instance.ChanceUp;
        if (duelist.WinChance > 100)
        {
            duelist.WinChance = 100;
        }
    }

    [MethodRpc((uint)Rpcs.RpcDuel)]
    public static void RpcDuel(PlayerControl player, PlayerControl target)
    {
        if (player.Data.Role is not Duelist)
        {
            Logger<OWPlugin>.Error("RpcDuel - Invalid Duelist");
            return;
        }

        var duelist = player.GetRole<Duelist>();
		duelist.RandomNumber = Random.Range(0, 100);

        DuelController.Begin(duelist.Player, target, duelist.WinChance);
    }

    public void Function(PlayerControl target, int Button)
    {
        if (Button == 1)
        {
            RpcSharpen(Player);
            if (Player.AmOwner) OWAssets.PlaySound(OWAssets.Duelist_Sharpen_SFX);
        }
        else if (Button == 2)
        {
            RpcDuel(Player, target);
        }
    }

    public int WinChance = (int)OptionGroupSingleton<Duelist_Options>.Instance.InitialChance;
    public int RandomNumber;
}

public sealed class Duelist_Sharpen : ObjectWorkshopRoleButton<Duelist>
{
    public override string Name => "Sharpen";
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => RoleColors.Crewmate;
    public override float Cooldown => OptionGroupSingleton<Duelist_Options>.Instance.Cooldown;
    public override LoadableAsset<Sprite> Sprite => OWAssets.KillSprite;

    protected override void OnClick() => VisitingMechanic.CheckVisit(Player, Player, 1, false, false);
    public override bool CanUse()
    { 
        return base.CanUse() && Role.WinChance < 100;
    }
}

public sealed class Duelist_Duel : ObjectWorkshopRoleButton<Duelist, PlayerControl>
{
    public override string Name => "Duel";
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => RoleColors.Crewmate;
    public override float Cooldown => OptionGroupSingleton<Duelist_Options>.Instance.DuelCD;
    public override LoadableAsset<Sprite> Sprite => OWAssets.KillSprite;

    public override void CreateButton(Transform parent)
    {
        base.CreateButton(parent);
        if (KeybindIcon != null) KeybindIcon.transform.localPosition = new Vector3(0.4f, 0.45f, -9f);
    }

    protected override void FixedUpdate(PlayerControl playerControl)
    {
        if (playerControl.IsRole<Duelist>())
        {
            Button?.usesRemainingText.gameObject.SetActive(true);
            Button?.usesRemainingSprite.gameObject.SetActive(true);
            Button!.usesRemainingText.text = Role.WinChance + "%";
        }

        base.FixedUpdate(playerControl);
    }

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance, predicate: x => x.IsTargetable() && !x.HasModifier<DuelingModifier>());
    }

    protected override void OnClick() => VisitingMechanic.CheckVisit(Player, Target, 2, false, true);
}

public sealed class Duelist_Options : AbstractOptionGroup<Duelist>
{
    public override string GroupName => "Duelist";

    [ModdedNumberOption("<color=#b3ffff>Duelist</color> <color=#4a86e8>Sharpen</color> Cooldown", 0, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float Cooldown { get; set; } = 25;

    [ModdedNumberOption("<color=#b3ffff>Duelist</color> <color=#4a86e8>Duel</color> Cooldown", 0, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float DuelCD { get; set; } = 25;

    [ModdedNumberOption("<color=#b3ffff>Duelist</color> <color=#4a86e8>Duel</color> Duration", 0.25f, 60f, 0.05f, MiraNumberSuffixes.Seconds, "0.00")]
    public float Duration { get; set; } = 0.5f;

    [ModdedNumberOption("<color=#b3ffff>Duelist</color> Win Chance Up", 0, 100f, 5f, MiraNumberSuffixes.Percent)]
    public float ChanceUp { get; set; } = 10f;

    [ModdedNumberOption("<color=#b3ffff>Duelist</color> Initial Win Chance", 0, 100f, 5f, MiraNumberSuffixes.Percent)]
    public float InitialChance { get; set; } = 50f;
}