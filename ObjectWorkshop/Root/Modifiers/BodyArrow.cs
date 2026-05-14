using TownOfUs.Modifiers;
using UnityEngine;

namespace ObjectWorkshop.Modifiers;

public sealed class BodyArrow(DeadBody deadBody, Color color) : ArrowDeadBodyModifier(deadBody, color, 0)
{
    public override string ModifierName => $"Amnesiac  Arrow";
}