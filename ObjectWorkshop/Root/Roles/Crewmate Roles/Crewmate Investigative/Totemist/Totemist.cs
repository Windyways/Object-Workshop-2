using System.Text;
using UnityEngine;
using Color = UnityEngine.Color;

namespace ObjectWorkshop.Roles;

public sealed class Totemist(IntPtr cppPtr) 
    : CrewmateRole(cppPtr), ICustomAURole, IWikiDiscoverable
{
    public string RoleName { get; set; } = "Totemist";
    public string RoleDescription => "Install totems, and watch them!";
    public string RoleLongDescription => RoleDescription;
    public Color RoleColor { get; set; } = RoleColors.Crewmate;

    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public Faction Faction { get; set; } = Faction.Crewmate;
    public CalculatedFaction CalculatedFaction { get; set; } = CalculatedFaction.Crewmate;
    public Alignment Alignment => Alignment.CrewmateInvestigative;

    public string RevealText => "is scared of the dark.";
    public string Description => "Radiate an area to increase the vision and speed of players within.";
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
        new("Install",
            "You can Install a totem into the ground during the round.\n" +
            "You will place down a totem, visible to everyone.",
            OWAssets.KillSprite),
            
        new("Watch",
            "You can Watch your totems during the round.\n" +
            "You will be immobile, and you will see around a selected totem.",
            OWAssets.KillSprite),
            
        new("Cycle",
            "You can Cycle between your totems during the round.\n" +
            "You will view a different totem by number.",
            OWAssets.KillSprite),
    ];

    public void Role_OnMeetingStart()
    {
        if (isWatching)
        {
            isWatching = false;

            LightSource light = Player.lightSource;
            light.transform.SetParent(Player.transform);
            light.transform.localPosition = Player.Collider.offset;
        }

        totemViewing = 1;
    }

    public void LobbyStart()
    {
        Totem.CleanUp();
    }

    [MethodRpc((uint)Rpcs.RpcInstall)]
    public static void RpcInstall(PlayerControl player)
    {
        Totem totem = Totem.Begin(player);
        if (player.Data.Role is Totemist totemist) totemist.TotemOrder.Add(totem);
    }

    public void Function(PlayerControl target, int Button)
    {
        if (Button == 1) RpcInstall(Player);
        if (Button == 2)
        {
            if (!isWatching) isWatching = true;
            else
            {
                isWatching = false;

                LightSource light = Player.lightSource;
                light.transform.SetParent(Player.transform);
                light.transform.localPosition = Player.Collider.offset;
            }
        }
        if (Button == 3)
        {
            totemViewing++;
            if (totemViewing > TotemOrder.Count) totemViewing = 1;
            if (isWatching)
            {
                GameObject targetTotem = TotemOrder[totemViewing - 1].gameObject;
                targetTotem.SetCameraToObject(Player);
            }
        }
    }

    public bool isWatching;
	public List<Totem> TotemOrder = new List<Totem>();
	public int totemViewing = 1;
}

public sealed class Totemist_Install : ObjectWorkshopRoleButton<Totemist>
{
    public override string Name => "Install";
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => RoleColors.Crewmate;
    public override float Cooldown => OptionGroupSingleton<Totemist_Options>.Instance.Cooldown;
    public override LoadableAsset<Sprite> Sprite => OWAssets.KillSprite;
    public override int MaxUses => (int)OptionGroupSingleton<Totemist_Options>.Instance.MaxInstalls;

    protected override void OnClick() => VisitingMechanic.CheckVisit(Player, Player, 1, false, false);
    public override bool CanUse()
    { 
        return base.CanUse() && !Role.isWatching;
    }
}

public sealed class Totemist_Watch : ObjectWorkshopRoleButton<Totemist>
{
    public override string Name => "Watch";
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => RoleColors.Crewmate;
    public override float Cooldown => 1f;
    public override LoadableAsset<Sprite> Sprite => OWAssets.KillSprite;

    protected override void OnClick() => VisitingMechanic.CheckVisit(Player, Player, 2, false, false);
    public override bool CanUse()
    { 
        return base.CanUse() && Role.TotemOrder.Count > 0;
    }
}

public sealed class Totemist_Cycle : ObjectWorkshopRoleButton<Totemist>
{
    public override string Name => "Cycle";
    public override BaseKeybind Keybind => Keybinds.TertiaryAction;
    public override Color TextOutlineColor => RoleColors.Crewmate;
    public override float Cooldown => 0.25f;
    public override LoadableAsset<Sprite> Sprite => OWAssets.KillSprite;

    protected override void OnClick() => VisitingMechanic.CheckVisit(Player, Player, 3, false, false);
    public override bool CanUse()
    { 
        return base.CanUse() && Role.isWatching && Role.TotemOrder.Count >= 2;
    }
}

public sealed class Totemist_Options : AbstractOptionGroup<Totemist>
{
    public override string GroupName => "Totemist";

    [ModdedNumberOption("<color=#b3ffff>Totemist</color> <color=#4a86e8>Install</color> Cooldown", 0f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float Cooldown { get; set; } = 25f;

    [ModdedNumberOption("<color=#b3ffff>Totemist</color> Max <color=#4a86e8>Installs</color>", 1f, 15f, 1f)]
    public float MaxInstalls { get; set; } = 3f;
}