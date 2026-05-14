using System.Text;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.UI.GridLayoutGroup;

namespace ObjectWorkshop.Roles;

public sealed class Ambiguator(IntPtr cppPtr)
    : NeutralRole(cppPtr), ICustomAURole, IWikiDiscoverable
{
    public string RoleName { get; set; } = "Ambiguator";
    public string RoleDescription => "Locate your Associate and Incubate others!";
    public string RoleLongDescription => RoleDescription;
    public Color RoleColor { get; set; } = RoleColors.Ambiguator;

    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public Faction Faction { get; set; } = Faction.Neutral;
    public CalculatedFaction CalculatedFaction { get; set; } = CalculatedFaction.Neutral;
    public Alignment Alignment => Alignment.NeutralAssociative;

    public string RevealText => "seems half-insectivorous.";
    public string Description => $"Locate to find your Associate. Determine to declare them, then you may Encapsulate foes in eggs to kill them. Ensure your Associate’s victory.";
    public string Intro => "You are a symbiote that requires a mate to start trapping those within eggs.";
    public string VictoryCondition => $"Make sure your Associate is victorious. You will win with your Associate.";

    public CustomRoleConfiguration Configuration => new(this)
    {
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>(),
        Icon = OWAssets.Ambiguator,
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
        new("Locate",
            "You can attempt to Locate your Associate during the round.\n" +
            "You will receive an arrow at their current location.",
            OWAssets.KillSprite),

        new("Determine",
            "You can Determine who you think your Associate is during the round.\n" +
            "If your target is your Associate, you will learn each other’s identity and you will unlock the ability to Incubate.",
            OWAssets.KillSprite),

        new("Encapsulate",
            "You can Encapsulate a player during the round.\n" +
            "You will lock them inside of a sturdy egg, and they will die in a certain amount of time, if a meeting begins, or when your Associate breaks it.\n" +
            "You may only Incubate once per round and only when you find your Associate.",
            OWAssets.KillSprite),
    ];

    public string GetPassives()
    {
        return
            "- At the start of the game, you are assigned an Associate.\n" +
            "- If your Associate dies before you find them while you are alive, you are assigned a new one.\n" +
            "- You have a Shield to protect you from the first attack you receive. The Shield is removed upon Determining an Associate.";
    }

    [MethodRpc((uint)Rpcs.RpcDetermine)]
    public static void RpcDetermine(PlayerControl player, PlayerControl target)
    {
        if (player.Data.Role is not Ambiguator)
        {
            Logger<OWPlugin>.Error("RpcDetermine - Invalid Ambiguator");
            return;
        }

        if (target.IsAssociate(player) && player.Data.Role is Ambiguator ambiguator)
        {
            ambiguator.FoundAssociate = true;
            player.RpcRemoveModifier<Shielded>();

            target.RpcAddModifier<RoleLearn>(player, false);
            player.RpcAddModifier<RoleLearn>(target, false);

            if (target.AmOwner()) target.Notify(Ambiguator_Feedback.NotifyAssociate(player), NotifyMode.InstantlyAndMeeting);
            if (player.AmOwner()) player.Notify(Ambiguator_Feedback.SetAssociate(target), NotifyMode.InstantlyAndMeeting);
        }
        else if (player.AmOwner()) player.Notify(Ambiguator_Feedback.NotAssociate(target), NotifyMode.InstantlyAndMeeting);
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        if (player.AmOwner() && OptionGroupSingleton<Ambiguator_Options>.Instance.EnableShield) player.RpcAddModifier<Shielded>();
    }

    public void Role_OnRoundStart(bool intro)
    {
        if (intro) AssignAssociate();
    }

    [MethodRpc((uint)Rpcs.RpcEncapsulate)]
    public static void RpcEncapsulate(PlayerControl player, PlayerControl target)
    {
        target.Immobilize();
        target.AddModifier<InvisibleTogglable>();
        SturdyEgg.Begin(player, target);
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        var Associate = Player.GetAssociate();
        if (Associate == null) return false;
        return Associate.Data.Role.DidWin(gameOverReason);
    }

    public override bool CanUse(IUsable usable)
    {
        if (!GameManager.Instance.LogicUsables.CanUse(usable, Player))
        {
            return false;
        }

        var console = usable.TryCast<Console>()!;
        return console == null || console.AllowImpostor;
    }

    public void AssignAssociate()
    {
        var validTarget = PlayerControl.AllPlayerControls.ToArray().Where(x => !x.HasDied() && !x.Is(Alignment.NeutralAssociative)).Random();
        if (validTarget == null) OWPlugin.DebugLogMessage("Ambiguator couldn't be assigned an Associate.", OWPlugin.MsgType.Error);
        else
        {
            ModifierUtils.GetPlayersWithModifier<Associate>(x => x.Caster == Player).FirstOrDefault()?.RpcRemoveModifier<Associate>();

            validTarget.RpcAddModifier<Associate>(Player);
            OWPlugin.DebugLogMessage($"Ambiguator's ({Player.Name()}) Associate is {validTarget.Name()}!");
        }
    }

    public void LobbyStart()
    {
        SturdyEgg.CleanUp();
    }

    public void Role_OnMeetingStart()
    {
        RemoveArrow();
        foreach (var sturdyEggs in SturdyEgg.AllSturdyEggs.Where(se => se.Owner == Player)) sturdyEggs.SetOff();
    }

    public void AddArrow()
    {
        RemoveArrow();

        var Associate = Player.GetAssociate();
        _arrow = MiscUtils.CreateArrow(Associate.gameObject.transform, RoleColors.Ambiguator);
        _arrow.target = Associate.transform.position;
        _arrow.Update();
    }

    public void RemoveArrow()
    {
        if (_arrow != null)
        {
            _arrow?.gameObject.Destroy();
            _arrow?.Destroy();
        }
    }

    public void Role_OnDeath(PlayerControl? player)
    {
        var Associate = Player.GetAssociate();
        if (Associate != null && player != null)
        {
            if (Associate == player && player.AmOwner() && !FoundAssociate && !Player.HasDied())
            {
                player.Notify(Ambiguator_Feedback.AssigningNewAssociate(), NotifyMode.InstantlyAndMeeting);
                AssignAssociate();
            }
        }
    }

    public void Function(PlayerControl target, int Button)
    {
        if (Button == 1) AddArrow();
        else if (Button == 2) RpcDetermine(Player, target);
        else if (Button == 3) RpcEncapsulate(Player, target);
    }

    public void FixedUpdate()
    {
        if (!OWPlugin.InGame())
            return;

        if (Player == null || _arrow == null)
            return;

        if (Debugger.IsDebuggerActive) // MCI patch.
        {
            var sr = _arrow.GetComponent<SpriteRenderer>();
            if (PlayerControl.LocalPlayer == Player) sr.Show();
            else sr.Hide();
        }
    }

    public bool FoundAssociate;
    private ArrowBehaviour? _arrow;
}

public sealed class Ambiguator_Locate : ObjectWorkshopRoleButton<Ambiguator>
{
    public override string Name => "Locate";
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => RoleColors.Ambiguator;
    public override float Cooldown => OptionGroupSingleton<Ambiguator_Options>.Instance.Cooldown;
    public override LoadableAsset<Sprite> Sprite => OWAssets.KillSprite;

    protected override void OnClick() => VisitingMechanic.CheckVisit(Player, Player, 1, false, false);
}

public sealed class Ambiguator_Determine : ObjectWorkshopRoleButton<Ambiguator, PlayerControl>
{
    public override string Name => "Determine";
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => RoleColors.Ambiguator;
    public override float Cooldown => OptionGroupSingleton<Ambiguator_Options>.Instance.DetermineCD;
    public override LoadableAsset<Sprite> Sprite => OWAssets.KillSprite;
    public override int MaxUses => (int)OptionGroupSingleton<Ambiguator_Options>.Instance.Charges;

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance);
    }

    protected override void OnClick() => VisitingMechanic.CheckVisit(Player, Target, 2, false, true);
    public override bool CanUse()
    {
        return base.CanUse() && !Role.FoundAssociate;
    }

    public override bool Enabled(RoleBehaviour? role)
    {
        return base.Enabled(role) && !Role.FoundAssociate;
    }
}

public sealed class Ambiguator_Encapsulate : ObjectWorkshopRoleButton<Ambiguator, PlayerControl>
{
    public override string Name => "Encapsulate";
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => RoleColors.Ambiguator;
    public override float Cooldown => OptionGroupSingleton<Ambiguator_Options>.Instance.EncapsulateCD;
    public override float EffectDuration => OptionGroupSingleton<Ambiguator_Options>.Instance.KillTimer;
    public override LoadableAsset<Sprite> Sprite => OWAssets.KillSprite;
    public override int MaxUses => 1;
    public override ButtonUsesMode UsesMode => ButtonUsesMode.PerRound;

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance, predicate: x =>
            Player.GetAssociate() != x);
    }

    protected override void OnClick() => VisitingMechanic.CheckVisit(Player, Target, 3, false, true);
    public override bool CanUse()
    {
        return base.CanUse() && Role.FoundAssociate;
    }

    public override bool Enabled(RoleBehaviour? role)
    {
        return base.Enabled(role) && Role.FoundAssociate;
    }
}

public static class Ambiguator_Feedback
{
    public static string SetAssociate(PlayerControl player)
    {
        return $"You have found {player.Name()} as your Associate! You may now Encapsulate!".ApplyKeywords();
    }

    public static string AssigningNewAssociate()
    {
        return $"Your Associate has perished, so you were assigned a new one!".ApplyKeywords();
    }

    public static string NotifyAssociate(PlayerControl player)
    {
        return $"{player.Name()}, the Ambiguator, has found you as their soulmate!".ApplyKeywords();
    }

    public static string NotAssociate(PlayerControl player)
    {
        return $"You Determined {player.Name()} as your Associate, but they weren't the one!".ApplyKeywords();
    }
}

public sealed class Ambiguator_Options : AbstractOptionGroup<Ambiguator>
{
    public override string GroupName => "Ambiguator";

    [ModdedNumberOption("<color=#57a284>Ambiguator</color> <color=#4a86e8>Locate</color> Cooldown", 0, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float Cooldown { get; set; } = 25;

    [ModdedNumberOption("<color=#57a284>Ambiguator</color> <color=#4a86e8>Determine</color> Cooldown", 0, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float DetermineCD { get; set; } = 25;

    [ModdedNumberOption("<color=#57a284>Ambiguator</color> <color=#4a86e8>Encapsulate</color> Cooldown", 0, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float EncapsulateCD { get; set; } = 25;

    [ModdedNumberOption("<color=#57a284>Ambiguator</color> <color=#4a86e8>Encapsulate</color> Kill Timer", 0, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float KillTimer { get; set; } = 15f;

    [ModdedNumberOption("<color=#57a284>Ambiguator</color> Max <color=#4a86e8>Determines</color>", 1, 15, 1f, MiraNumberSuffixes.None)]
    public float Charges { get; set; } = 3f;

    [ModdedToggleOption("Enable <color=#57a284>Ambiguator</color> <color=#0000ff>Shield</color>")]
    public bool EnableShield { get; set; } = true;
}