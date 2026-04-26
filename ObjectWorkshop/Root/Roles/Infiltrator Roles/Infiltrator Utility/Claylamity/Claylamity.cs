using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ObjectWorkshop.Roles;

public sealed class Claylamity(IntPtr cppPtr)
    : NeutralRole(cppPtr), ICustomAURole, IWikiDiscoverable, IVisualAppearance
{
    public string RoleName { get; set; } = "Claylamity";
    public string RoleDescription => "Metamorphosis into crew abilities.";
    public string RoleLongDescription => RoleDescription;
    public Color RoleColor { get; set; } = RoleColors.Claylamity;

    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public Faction Faction { get; set; } = Faction.Infiltrator;
    public CalculatedFaction CalculatedFaction { get; set; } = CalculatedFaction.Neutral;
    public Alignment Alignment => Alignment.InfiltratorUtility;

    public string RevealText => "can transform into any command.";
    public string Description => $"Morph your second ability into a visual Crewmate ability. You are seen as a Neutral to others.";
    public string Intro => "You are a clay oddity that morphs its skills into others to fit in secretly.";
    public string VictoryCondition => $"Kill everyone that opposes the Infiltrators. You will win with other Infiltrator members.";

    public CustomRoleConfiguration Configuration => new(this)
    {
        CanUseSabotage = OptionGroupSingleton<InfiltratorOptions>.Instance.CanSabotage,
        CanUseVent = OptionGroupSingleton<InfiltratorOptions>.Instance.CanVent,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>(),
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
        new("SUbMerGE",
            "You can Submerge a player in goo during the round.\n" +
            "You will kill your target. Your target will be slower than usual as a ghost.",
            OWAssets.KillSprite),

        new($"Metamorphosis",
            "You can Metamorphosize your ability during the round.\n" +
            "You can choose between any crewmate ability that displays a visual.\n" +
            "You will inherit the ability for the rest of the game, or until you morph it into a new one.\n" +
            "Other players see you as a Neutral rather than an Infiltrator.",
            OWAssets.KillSprite)
    ];

    public bool WinConditionMet() => InfiltratorGameOver.WinConditionMet();
    public override bool DidWin(GameOverReason gameOverReason)
    {
        return WinConditionMet() || InfiltratorGameOver.AnyWon(gameOverReason);
    }

    public RoleBehaviour metamorphosisRole;
    public void MetamorphosisMenu()
    {
        if (Minigame.Instance != null)
            return;

        var shapeMenu = GuesserMenu.Create();
        shapeMenu.Begin(IsRoleValid, ClickRoleHandle);

        void ClickRoleHandle(RoleBehaviour role)
        {
            metamorphosisRole = role;
            if (Player.AmOwner())
            {
                var button = CustomButtonSingleton<Claylamity_Metamorphosis>.Instance;
                button.DecreaseUses();
            }

            shapeMenu.Close();
        }
    }

    private static bool IsRoleValid(RoleBehaviour role) // Which roles can Claylamity use abilities from.
    {
        if (role.IsDead) return false;
        if (role is IGhostRole) return false;

        if (role is Duelist) return true;
        //if (role is Totemist) return true;
        //if (role is GiftWeaver) return true;
        if (role is UFO) return true;
        if (role is Oasis) return true;
        //if (role is Prognosticator) return true;
        //if (role is Titan) return true;
        if (role is Luminescence) return true;

        return false;
    }

    public void LobbyStart()
    {
        // In case other roles arent there to clean it up.
        AlarmClock.CleanUp();
        //Orb.CleanUp();
        //Totem.CleanUp();
        Sandstorm.CleanUp();
        //Crate.CleanUp();
        Moon.CleanUp();
        Lightbulb.CleanUp();
    }

    // TOTEMIST
    /*[MethodRpc((uint)OWRpc.RpcInstall, SendImmediately = true)]
    public static void RpcInstall(PlayerControl player)
    {
        if (player.Data.Role is not Claylamity)
        {
            Logger<OWPlugin>.Error("RpcInstall - Invalid Claylamity");
            return;
        }

        var claylamity = player.GetRole<Claylamity>();

        Totem.Begin(claylamity.Player);
    }*/

    // DUELIST
    public int RandomNumber;
    public int WinChance = (int)OptionGroupSingleton<Duelist_Options>.Instance.InitialChance;

    [MethodRpc((uint)Rpcs.RpcDuel)]
    public static void RpcDuel(PlayerControl player, PlayerControl target)
    {
        if (player.Data.Role is not Claylamity)
        {
            Logger<OWPlugin>.Error("RpcDuel - Invalid Claylamity");
            return;
        }

        var claylamity = player.GetRole<Claylamity>();
        claylamity.RandomNumber = Random.Range(0, 100);

        DuelController.Begin(player, target, claylamity.WinChance);
    }

    // UFO
    [MethodRpc((uint)Rpcs.RpcDestination)]
    public static void RpcDestination(PlayerControl player)
    {
        if (player.Data.Role is not Claylamity)
        {
            Logger<OWPlugin>.Error("PlaceMoon - Invalid Claylamity");
            return;
        }

        Moon.DestroyAll();
        Moon.Begin(player);
    }

    // GIFT WEAVER
    /*[MethodRpc((uint)OWRpc.RpcPackage, SendImmediately = true)]
    public static void RpcPackage(PlayerControl player)
    {
        if (player.Data.Role is not Claylamity)
        {
            Logger<OWPlugin>.Error("RpcPackage - Invalid Claylamity");
            return;
        }

        var claylamity = player.GetRole<Claylamity>();
        Crate.Begin(claylamity.Player);
    }*/

    /*[MethodRpc((uint)OWRpc.RpcOpenCrate, SendImmediately = true)]
    public static void RpcOpenCrate(PlayerControl player, PlayerControl openClient)
    {
        if (player.Data.Role is not Claylamity)
        {
            Logger<OWPlugin>.Error("RpcOpenCrate - Invalid Claylamity");
            return;
        }

        var claylamity = player.GetRole<Claylamity>();
        var crate = Crate.GetObjectByOwner(claylamity.Player);

        if (player == null)
        {
            Crate.AllCrates.Remove(crate);
            Destroy(crate.gameObject);
        }
        else
        {
            if (crate.gameObject != null)
            {
                if (openClient != player)
                {
                    if (openClient.Is(Faction.Crewmate))
                    {
                        MiscUtils.CompleteRandomTask(openClient);
                        openClient.RpcAddModifier<RoleLearn>(player, true, true);
                    }
                    else
                    {
                        if (openClient.Is(Faction.Infiltrator, true))
                        {
                            if (openClient.AmOwner()) AudioSource.PlayClipAtPoint(HudManager.Instance.TaskUpdateSound, openClient.transform.position);
                            if (player.AmOwner())
                            {
                                MiscUtils.ShowNotification(GiftWeaver.Info(), Color.white);
                                MiscUtils.AddFakeChat(player.CachedPlayerData, MiscUtils.GetTitle(claylamity), GiftWeaver.Info());
                            }
                        }
                        else if (openClient.AmOwner())
                        {
                            AudioSource.PlayClipAtPoint(HudManager.Instance.TaskUpdateSound, openClient.transform.position);
                        }
                    }
                }

                Crate.AllCrates.Remove(crate);
                Destroy(crate.gameObject);
            }
        }
    }*/

    // PROGNOSTICATOR
    /*public void RevealRandomEvil()
    {
        List<PlayerControl> candidates = new List<PlayerControl>();
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            if (player.Is(Faction.Infiltrator) || player.Is(Faction.Neutral))
            {
                if (!player.Is(Alignment.NeutralAssociative) && !player.HasModifier<RoleLearn>(x => x.Owner == Player))
                {
                    candidates.Add(player);
                }
            }
        }

        candidates.Shuffle();
        var target = candidates[0];

        target.RpcAddModifier<RoleLearn>(Player, true, false);
        if (Debugger.IsDebuggerActive)
        {
            if (target.CanAddModifier<EvidenceAgainst>()) target.AddModifier<EvidenceAgainst>(Player, true);
        }
    }*/

    // TITAN
    private float currentScale = 0.7f;
    public bool isGiant;
    public void SetGiant(bool giant)
    {
        //WitnessKillEvent.WitnessGood(Player, Player.GetTruePosition(), "Turned Giant");

        isGiant = giant;
        currentScale = giant ? 1.25f : 0.7f; // set goal size
        Player.RawSetAppearance(this);
    }

    public VisualAppearance GetVisualAppearance()
    {
        var appearance = Player.GetDefaultAppearance();

        if (isGiant) appearance.Speed = 1f; // slower when big
        appearance.Size = new Vector3(currentScale, currentScale, 1f);

        return appearance;
    }

    // LUMINESCENCE
    [MethodRpc((uint)Rpcs.RpcRadiate)]
    public static void RpcRadiate(PlayerControl player)
    {
        if (player.Data.Role is not Claylamity)
        {
            Logger<OWPlugin>.Error("RpcRadiate - Invalid Claylamity");
            return;
        }

        Lightbulb.Begin(player);
    }

    public void Function(PlayerControl target, int Button)
    {
        if (Button == 1)
        {
            if (Player.CanKill(target))
            {
                target.RpcAddModifier<Submerged>();
                Player.RpcCustomMurder(target);
                VisitingMechanic.RpcAddDeathReason(target, (int)DeathReasonShow.Submerged);
            }
        }
        else if (Button == 2)
        {
            if (metamorphosisRole is Duelist) RpcDuel(Player, target);
            else if (metamorphosisRole is UFO) RpcDestination(Player);
            else if (metamorphosisRole is Luminescence) RpcRadiate(Player);
            else if (metamorphosisRole is Oasis) Oasis.RpcStartSandstorm(Player);
        }
        else if (Button == 3) MetamorphosisMenu();
    }
}

public sealed class Claylamity_Attack : ObjectWorkshopRoleButton<Claylamity, PlayerControl>
{
    public override string Name => "Attack";
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => RoleColors.Claylamity;
    public override float Cooldown => OptionGroupSingleton<InfiltratorOptions>.Instance.Cooldown;
    public override LoadableAsset<Sprite> Sprite => OWAssets.KillSprite;

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(false, Distance, predicate: x => 
            x.IsTargetable() && !x.Is(Faction.Infiltrator));
    }

    protected override void OnClick() => VisitingMechanic.CheckVisit(Player, Target, 1, true, true);
}

public sealed class Claylamity_Metamorphosis : ObjectWorkshopRoleButton<Claylamity>
{
    public override string Name => "Metamorphosis";
    public override BaseKeybind Keybind => Keybinds.TertiaryAction;
    public override Color TextOutlineColor => RoleColors.Claylamity;
    public override float Cooldown => 0.1f;
    public override int MaxUses => (int)OptionGroupSingleton<Claylamity_Options>.Instance.Charges;
    public override LoadableAsset<Sprite> Sprite => OWAssets.KillSprite;
    public override ButtonLocation Location => ButtonLocation.BottomLeft;
    public override bool DecreaseCharge => false;

    protected override void OnClick() => VisitingMechanic.CheckVisit(Player, Player, 3, false, false);
}

public sealed class Claylamity_Options : AbstractOptionGroup<Claylamity>
{
    public override string GroupName => "Claylamity";

    [ModdedNumberOption("<color=#5b5353>Claylamity</color> Max <color=#4a86e8>Metamorphosizes</color>", 1f, 15f, 1f, MiraNumberSuffixes.None)]
    public float Charges { get; set; } = 2;

    [ModdedNumberOption("<color=#5b5353>Claylamity</color> Victim Speed", 0.25f, 5f, 0.05f, MiraNumberSuffixes.Multiplier, "0.00")]
    public float Speed { get; set; } = 0.75f;
}

// ROLE ABILITIES
/*public sealed class Claylamity_Install : ObjectWorkshopRoleButton<Claylamity>
{
    public override string Name => "Install";
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => OWColors.Crewmate;
    public override float Cooldown => OptionGroupSingleton<Totemist_Options>.Instance.Cooldown + MapCooldown;
    public override LoadableAsset<Sprite> Sprite => OWAssets.KillSprite;
    public override int MaxUses => (int)OptionGroupSingleton<Totemist_Options>.Instance.MaxInstalls;

    public override void ClickHandler()
    {
        if (Timer <= 0)
        {
            if (MiscUtils.SuccessfulVisit(Player, Player, isAttacking: false, isVisiting: false))
            {
                base.ClickHandler();
            }
        }
    }

    protected override void OnClick()
    {
        Claylamity.RpcInstall(Role.Player);
    }

    public override bool Enabled(RoleBehaviour? role)
    {
        return base.Enabled(role) && Role.metamorphosisRole is Totemist;
    }
}*/

public sealed class Claylamity_Duel : ObjectWorkshopRoleButton<Claylamity, PlayerControl>
{
    public override string Name => "Duel";
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => RoleColors.Claylamity;
    public override float Cooldown => OptionGroupSingleton<Duelist_Options>.Instance.DuelCD;
    public override LoadableAsset<Sprite> Sprite => OWAssets.KillSprite;

    public override void CreateButton(Transform parent)
    {
        base.CreateButton(parent);
        if (KeybindIcon != null) KeybindIcon.transform.localPosition = new Vector3(0.4f, 0.45f, -9f);
    }

    protected override void FixedUpdate(PlayerControl playerControl)
    {
        if (playerControl == Player)
        {
            Button?.usesRemainingText.gameObject.SetActive(true);
            Button?.usesRemainingSprite.gameObject.SetActive(true);
            Button!.usesRemainingText.text = Role.WinChance + "%";
        }

        base.FixedUpdate(playerControl);
    }

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(false, Distance, predicate: x => 
            !x.Is(Faction.Infiltrator) && x.IsTargetable() && !x.HasModifier<DuelingModifier>());
    }

    protected override void OnClick() => VisitingMechanic.CheckVisit(Player, Target, 2, false, true);
    public override bool Enabled(RoleBehaviour? role)
    {
        return base.Enabled(role) && Role.metamorphosisRole is Duelist;
    }
}

public sealed class Claylamity_Sandstorm : ObjectWorkshopRoleButton<Claylamity>
{
    public override string Name => "Sandstorm";
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => RoleColors.Claylamity;
    public override float Cooldown => OptionGroupSingleton<Oasis_Options>.Instance.SandstormCD;
    public override LoadableAsset<Sprite> Sprite => OWAssets.KillSprite;
    public override float EffectDuration => OptionGroupSingleton<Oasis_Options>.Instance.SandstormDuration;

    protected override void OnClick() => VisitingMechanic.CheckVisit(Player, Player, 2, false, false);
    public override bool Enabled(RoleBehaviour? role)
    {
        return base.Enabled(role) && Role.metamorphosisRole is Oasis;
    }
}

public sealed class Claylamity_Destination : ObjectWorkshopRoleButton<Claylamity>
{
    public override string Name => "Destination";
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => RoleColors.Claylamity;
    public override float Cooldown => OptionGroupSingleton<UFO_Options>.Instance.Cooldown;
    public override LoadableAsset<Sprite> Sprite => OWAssets.KillSprite;

    protected override void OnClick() => VisitingMechanic.CheckVisit(Player, Player, 2, false, false);
    public override bool CanUse()
    {
        var moon = Moon.GetMoon();
        if (moon == null) return base.CanUse();
        return base.CanUse() && !Moon.GetMoon().isAbductOccuring;
    }

    public override bool Enabled(RoleBehaviour? role)
    {
        return base.Enabled(role) && Role.metamorphosisRole is UFO;
    }
}

/*public sealed class Claylamity_Package : ObjectWorkshopRoleButton<Claylamity>
{
    public override string Name => "Package";
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => OWColors.Crewmate;
    public override float Cooldown => OptionGroupSingleton<GiftWeaver_Options>.Instance.Cooldown;
    public override LoadableAsset<Sprite> Sprite => OWAssets.KillSprite;

    public override void ClickHandler()
    {
        if (Timer <= 0)
        {
            if (MiscUtils.SuccessfulVisit(Player, Player, isAttacking: false, isVisiting: false))
            {
                base.ClickHandler();
            }
        }
    }

    protected override void OnClick()
    {
        Claylamity.RpcPackage(Player);
    }

    public override bool Enabled(RoleBehaviour? role)
    {
        return base.Enabled(role) && Role.metamorphosisRole is GiftWeaver;
    }
}*/

/*public sealed class Claylamity_Predict : ObjectWorkshopRoleButton<Claylamity, PlayerControl>
{
    public override string Name => "Predict";
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => OWColors.Crewmate;
    public override float Cooldown => OptionGroupSingleton<Prognosticator_Options>.Instance.Cooldown;
    public override LoadableAsset<Sprite> Sprite => OWAssets.KillSprite;

    public override void ClickHandler()
    {
        if (Target != null && Timer <= 0)
        {
            if (MiscUtils.SuccessfulVisit(Player, Target, isAttacking: false, isVisiting: true))
            {
                base.ClickHandler();
            }
        }
    }

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance, predicate: x => x.IsTargetable() && !x.HasModifier<PredictedModifier>(x => x.Owner == Player));
    }

    protected override void OnClick()
    {
        if (Target == null)
            return;

        Target.RpcAddModifier<PredictedModifier>(Player);
    }

    public override bool Enabled(RoleBehaviour? role)
    {
        return base.Enabled(role) && Role.metamorphosisRole is Prognosticator;
    }
}*/

/*public sealed class Claylamity_Grow : ObjectWorkshopRoleButton<Claylamity>
{
    public override string Name => "Grow";
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => OWColors.Crewmate;
    public override float Cooldown => OptionGroupSingleton<Titan_Options>.Instance.Cooldown;
    public override float EffectDuration => OptionGroupSingleton<Titan_Options>.Instance.Duration;
    public override LoadableAsset<Sprite> Sprite => OWAssets.KillSprite;

    public override void ClickHandler()
    {
        if (Timer <= 0)
        {
            if (MiscUtils.SuccessfulVisit(Player, Player, isAttacking: false, isVisiting: false))
            {
                base.ClickHandler();
            }
        }
    }

    protected override void OnClick()
    {
        if (Role.isGiant) Role.SetGiant(false);
        else Role.SetGiant(true);
    }

    public override void OnEffectEnd()
    {
        if (Role.isGiant) Role.SetGiant(false);
    }

    public override bool Enabled(RoleBehaviour? role)
    {
        return base.Enabled(role) && Role.metamorphosisRole is Titan;
    }
}*/

public sealed class Claylamity_Radiate : ObjectWorkshopRoleButton<Claylamity>
{
    public override string Name => "Radiate";
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => RoleColors.Claylamity;
    public override float Cooldown => OptionGroupSingleton<Luminescence_Options>.Instance.Cooldown;
    public override LoadableAsset<Sprite> Sprite => OWAssets.KillSprite;
    public override float EffectDuration => OptionGroupSingleton<Luminescence_Options>.Instance.Duration;
    public override int MaxUses => (int)OptionGroupSingleton<Luminescence_Options>.Instance.MaxShatters;
    public override bool DecreaseCharge => false;

    protected override void OnClick() => VisitingMechanic.CheckVisit(Player, Player, 2, false, false);
    public override bool Enabled(RoleBehaviour? role)
    {
        return base.Enabled(role) && Role.metamorphosisRole is Luminescence;
    }
}