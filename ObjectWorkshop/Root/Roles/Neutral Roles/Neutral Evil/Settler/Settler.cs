using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ObjectWorkshop.Roles;

public sealed class Settler(IntPtr cppPtr)
    : NeutralRole(cppPtr), ICustomAURole, IWikiDiscoverable
{
    public string RoleName { get; set; } = "Settler";
    public string RoleDescription => "Lay eggs at your Nest.";
    public string RoleLongDescription => RoleDescription;
    public Color RoleColor { get; set; } = RoleColors.Settler;

    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public Faction Faction { get; set; } = Faction.Neutral;
    public CalculatedFaction CalculatedFaction { get; set; } = CalculatedFaction.Neutral;
    public Alignment Alignment => Alignment.NeutralEvil;

    public string RevealText => "is oviparous.";
    public string Description => $"Find your nest and lay an egg {OptionGroupSingleton<Settler_Options>.Instance.Required} times to win.";
    public string Intro => "You are a planter of eggs that plans for domination after doing their daily work.";
    public string VictoryCondition => $"Find your Nest and lay {OptionGroupSingleton<Settler_Options>.Instance.Required} eggs. You will win with other Settlers.";

    public override float Vision() => OptionGroupSingleton<Settler_Options>.Instance.Vision;
    public CustomRoleConfiguration Configuration => new(this)
    {
        CanUseVent = OptionGroupSingleton<Settler_Options>.Instance.CanVent,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>(),
        Icon = OWAssets.Settler,
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
        new("Lay",
            "You can Lay an egg at the Nest during the round.\n" +
            "You will become invisible, invincibile, and immobile at the Nest until the next meeting.",
            OWAssets.KillSprite),
    ];

    public bool WinConditionMet()
    {
        if (Player.HasDied()) return false;

        var result = EggsLayed >= (int)OptionGroupSingleton<Settler_Options>.Instance.Required;
        return result;
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return WinConditionMet();
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

    public void LobbyStart()
    {
        Nest.CleanUp();
    }

    public void Role_OnRoundStart(bool intro)
    {
        Player.Mobilize();
        LayedEggsThisRound = false;

        // This is where we reroll Nest position.
        Nest.SpawnNest();
    }

    public void Role_OnMeetingStart()
    {
        if (Nest.currentNest != null)
        {
            Object.Destroy(Nest.currentNest.gameObject);
            Nest.currentNest = null;
        }
    }

    public void Function(PlayerControl target, int Button)
    {
        if (Button == 1)
        {
            var nest = Nest.currentNest;
            if (nest == null)
            {
                OWPlugin.DebugLogMessage("Nest does not exist!", OWPlugin.MsgType.Error);
                return;
            }

            nest.eggsInNest++;
            nest.SpawnEggs(1);

            EggsLayed++;
            LayedEggsThisRound = true;

            Player.Immobilize();
            Player.RpcAddModifier<InvisibleTogglable>();
        }
    }

    public int EggsLayed;
    public bool LayedEggsThisRound;
}

public sealed class Settler_Lay : ObjectWorkshopRoleButton<Settler>
{
    public override string Name => "Lay";
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => RoleColors.Settler;
    public override float Cooldown => 1f;
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
            Button!.usesRemainingText.text = Role.EggsLayed + "/" + (int)OptionGroupSingleton<Settler_Options>.Instance.Required;

            if (Button != null)
            {
                Button.usesRemainingSprite.gameObject.transform.localPosition = new Vector3(0.4f, -0.1f, -0.1f);
            }
        }

        base.FixedUpdate(playerControl);
    }

    protected override void OnClick() => VisitingMechanic.CheckVisit(Player, Player, 1, false, false);
    public override bool CanUse()
    {
        return !Role.LayedEggsThisRound && Nest.IsInRange(Player);
    }
}

public sealed class Settler_Options : AbstractOptionGroup<Settler>
{
    public override string GroupName => "Settler";

    [ModdedNumberOption("<color=#f0da6a>Settler</color> <color=#4a86e8>Eggs</color> Required", 1f, 10f, 1f, MiraNumberSuffixes.None)]
    public float Required { get; set; } = 3f;

    [ModdedNumberOption("<color=#f0da6a>Settler</color> Vision", 0.25f, 60f, 0.25f, MiraNumberSuffixes.Multiplier, "0.00")]
    public float Vision { get; set; } = 1f;

    [ModdedToggleOption("<color=#f0da6a>Settler</color> Can Vent")]
    public bool CanVent { get; set; } = true;
}