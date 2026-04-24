using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using Il2CppInterop.Runtime.Injection;
using MiraAPI;
using Reactor;
using Reactor.Localization;
using Reactor.Networking;
using System.Globalization;
using TownOfUs.Patches.Misc;
using static ObjectWorkshop.MCI.Reactor_Coroutines;
using ModCompatibility = TownOfUs.Modules.ModCompatibility;

namespace ObjectWorkshop;

/// <summary>
///     Plugin class for mod.
/// </summary>
[BepInAutoPlugin("windyways.objectworkshop2", "Object Workshop 2")]
[BepInProcess("Among Us.exe")]
[BepInDependency(ReactorPlugin.Id)]
[BepInDependency(MiraApiPlugin.Id)]
[ReactorModFlags(ModFlags.RequireOnAllClients)]
public partial class OWPlugin : BasePlugin, IMiraPlugin
{
    public static string BetaVersion = "Build 3";

    /// <summary>
    ///     Gets the specified Culture for string manipulations.
    /// </summary>
    public static CultureInfo Culture { get; } = new("en-US");

    /// <summary>
    ///     Gets the Harmony instance for patching.
    /// </summary>
    public Harmony Harmony { get; } = new(Id);

    public static ConfigEntry<float> ButtonUIFactor { get; set; }
    public static ConfigEntry<bool> OffsetButtons { get; set; }
    public static ConfigEntry<int> RoleIconSpot { get; set; }
    public static ConfigEntry<bool> ArachnophobiaMode { get; set; }

    /// <summary>
    ///     Determines if the current build is a dev build or not. This will change certain visuals as well as always grab news locally to be up to date.
    /// </summary>
    public static bool IsDevBuild => false;
    
    /// <inheritdoc />
    public string OptionsTitleText => "Object\nWorkshop 2";

    /// <inheritdoc />
    public ConfigFile GetConfigFile()
    {
        return Config;
    }

    public OWPlugin()
    {
        TouLocale.Initialize();
    }

    /// <summary>
    ///     The Load method for the plugin.
    /// </summary>
    public override void Load()
    {
        ReactorCredits.Register("Object Workshop 2", Version, IsDevBuild, ReactorCredits.AlwaysShow);
        LocalizationManager.Register(new TaskProvider());

        TouAssets.Initialize();

        IL2CPPChainloader.Instance.Finished +=
            ModCompatibility
                .Initialize; // Initialise AFTER the mods are loaded to ensure maximum parity (no need for the soft dependency either then)
        IL2CPPChainloader.Instance.Finished +=
            ModNewsFetcher.CheckForNews; // Checks for mod announcements after everything is loaded to avoid Epic Games crashing

        //var path = Path.GetDirectoryName(Assembly.GetAssembly(typeof(OWPlugin))!.Location) + "\\touhats.catalog";
        //AddressablesLoader.RegisterCatalog(path);
        //AddressablesLoader.RegisterHats("touhats");

        ButtonUIFactor = Config.Bind("LocalSettings", "ButtonUIFactor", 0.8f,
            "Scale factor for buttons in-game. Preferably, keep the value between 0.5f and 1.5f.");
        OffsetButtons = Config.Bind("LocalSettings", "OffsetButtons", false,
            "If venting is disabled (and you're not an impostor), should there be a blank spot where the vent button usually is?");

        RoleIconSpot = Config.Bind("LocalSettings", "IconPos", 0,
            "The position of Role Icons. 0 is next to the role name, 1 is next to the player name, 2 is to disable.");
        ArachnophobiaMode = Config.Bind("LocalSettings", "ArachnophobiaMode", false,
            "Enable Arachnophobia Mode. This replaces the Spider (from Arachnid) with a yellow circle.");

        Harmony.PatchAll();

        // For inbuilt MCI
        ClassInjector.RegisterTypeInIl2Cpp<Debugger>();
        ClassInjector.RegisterTypeInIl2Cpp<Component>();
        AddComponent<Component>();
        Debugger = AddComponent<Debugger>();

        RoleReferences.Initialize();
    }

    public enum MsgType { Message, Warning, Error }
    public static void DebugLogMessage(string message, MsgType type = MsgType.Message)
    {
        if (type == MsgType.Error) PluginSingleton<OWPlugin>.Instance.Log.LogError(message);
        else if (type == MsgType.Warning) PluginSingleton<OWPlugin>.Instance.Log.LogWarning(message);
        else if (type == MsgType.Message) PluginSingleton<OWPlugin>.Instance.Log.LogMessage(message);
    }

    public static bool InGame()
    {
        return AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started;
    }

    public static string RobotName { get; set; } = "Bot";
    public static List<PlayerControl> IsBot = new List<PlayerControl>();
    public static bool Persistence { get; set; } = true;
    public static Debugger Debugger { get; set; }
}