namespace ObjectWorkshop.Enums;

public enum DeathReasonShow
{
    None,
    Alive,
    Ejected,

    // Not role specific, i guess we can do death reasons here like TOH does? Won't be used tho because this mod is already quite Crew-sided.
    Killed, // Duelist, Reaper/Undead Reaper, Infiltrator Standard kill
    Shot, // Aimsman, Culverin
    Incinerated, // Pyre
    Haunted, // Gravekeeper, Specter
    Stomped, // Titan
    Devoured, // Enticer
    Bitten, // Schistocerca Locusts, Arachnid
    Submerged, // Claylim
    Skewered, // Weapon Master
}