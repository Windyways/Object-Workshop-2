using UnityEngine;

namespace ObjectWorkshop.Interfaces;

public interface INotThreatable
{

}

public interface IStatusEffect
{
    string Name { get; }
    Sprite Icon { get; }
    PlayerControl Source { get; set; }

    bool CanSee(PlayerControl viewer, PlayerControl target);
}