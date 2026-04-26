using UnityEngine;

namespace ObjectWorkshop.Modifiers;

public sealed class Anonymous : BaseModifier, IVisualAppearance
{
    public override string ModifierName => "Anonymous";
    public bool VisualPriority => true;

    public VisualAppearance GetVisualAppearance()
    {
        var appearance = Player.GetDefaultAppearance();
        appearance.Speed = 1f;
        appearance.Size = new Vector3(0.7f, 0.7f, 1f);
        appearance.ColorId = Player.Data.DefaultOutfit.ColorId;
        appearance.HatId = "hat_NoHat";
        appearance.SkinId = "skin_None";
        appearance.VisorId = "visor_EmptyVisor";
        appearance.PlayerName = string.Empty;
        appearance.PetId = "pet_EmptyPet";
        appearance.NameVisible = false;

        if (Player.IsPeacock()) appearance.PlayerMaterialColor = RoleColors.Peacock;
        else if (PeacockVisual.IsPlayerAnyParalyzed(Player)) appearance.PlayerMaterialColor = Color.white;
        else ModifierComponent!.RemoveModifier(this);

        return appearance;
    }

    public override void OnActivate()
    {
        Player.RawSetAppearance(this);
        if (PeacockVisual.IsPlayerAnyParalyzed(Player) && Player.AmOwner()) Player.Notify(Peacock_Feedback.VictimFroze(), NotifyMode.Instantly);
    }

    public override void OnDeactivate()
    {
        Player?.ResetAppearance();
    }

    public override void OnDeath(DeathReason reason)
    {
        base.OnDeath(reason);
        ModifierComponent!.RemoveModifier(this);
    }

    public override void OnMeetingStart()
    {
        ModifierComponent!.RemoveModifier(this);
    }
}