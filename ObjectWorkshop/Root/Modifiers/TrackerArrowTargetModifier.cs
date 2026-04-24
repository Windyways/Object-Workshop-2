using TownOfUs.Modifiers;
using TownOfUs.Modules.RainbowMod;
using UnityEngine;

namespace ObjectWorkshop.Modifiers;

public sealed class TrackerArrowTargetModifier(PlayerControl owner, Color color, float update)
    : ArrowTargetModifier(owner, color, update)
{
    public override string ModifierName => "Arrow";

    public override void OnActivate()
    {
        base.OnActivate();

        if (Arrow == null)
        {
            return;
        }

        var spr = Arrow.gameObject.GetComponent<SpriteRenderer>();
        var r = Arrow.gameObject.AddComponent<BasicRainbowBehaviour>();

        r.AddRend(spr, Player.cosmetics.ColorId);
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        if (Debugger.IsDebuggerActive)
        {
            var spr = Arrow.gameObject.GetComponent<SpriteRenderer>();
            if (PlayerControl.LocalPlayer == Owner) spr.Show();
            else spr.Hide();
        }
    }
}