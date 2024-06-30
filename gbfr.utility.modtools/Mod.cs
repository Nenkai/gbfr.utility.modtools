using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

using Reloaded.Mod.Interfaces;
using Reloaded.Hooks.Definitions;
using Reloaded.Memory.Sigscan;
using Reloaded.Memory.Sigscan.Definitions.Structs;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using IReloadedHooks = Reloaded.Hooks.ReloadedII.Interfaces.IReloadedHooks;

using DearImguiSharp;

using gbfr.utility.modtools.Configuration;
using gbfr.utility.modtools.Template;
using gbfr.utility.modtools.Hooks;
using gbfr.utility.modtools.Windows;
using gbfr.utility.modtools.Windows.Tables;

namespace gbfr.utility.modtools;

/// <summary>
/// Your mod logic goes here.
/// </summary>
public unsafe class Mod : ModBase // <= Do not Remove.
{
    /// <summary>
    /// Provides access to the mod loader API.
    /// </summary>
    private readonly IModLoader _modLoader;

    /// <summary>
    /// Provides access to the Reloaded.Hooks API.
    /// </summary>
    /// <remarks>This is null if you remove dependency on Reloaded.SharedLib.Hooks in your mod.</remarks>
    private readonly IReloadedHooks? _hooks;

    /// <summary>
    /// Provides access to the Reloaded logger.
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// Entry point into the mod, instance that created this class.
    /// </summary>
    private readonly IMod _owner;

    /// <summary>
    /// Provides access to this mod's configuration.
    /// </summary>
    private Config _configuration;

    /// <summary>
    /// The configuration of the currently executing mod.
    /// </summary>
    private readonly IModConfig _modConfig;

    private static IStartupScanner? _startupScanner = null!;

    private ImguiSupport _imguiSupport;
    private FileLogger _fileLogger;
    private GameStateHook _gameStateHook;

    private CharacterManagerHook _charManagerHook;
    private GemManagerHook _gemManagerHook;
    private ItemManagerHook _itemManagerHook;
    private LimitApManagerHook _limitApManagerHook;
    private SkillManagerHook _skillManagerHook;
    private WeaponManagerHook _weaponManagerHook;

    public Mod(ModContext context)
    {
        _modLoader = context.ModLoader;
        _hooks = context.Hooks;
        _logger = context.Logger;
        _owner = context.Owner;
        _configuration = context.Configuration;
        _modConfig = context.ModConfig;

        var startupScannerController = _modLoader.GetController<IStartupScanner>();
        if (startupScannerController == null || !startupScannerController.TryGetTarget(out _startupScanner))
        {
            return;
        }

#if DEBUG
        Debugger.Launch();
#endif
        _imguiSupport = new ImguiSupport(_hooks);
        _imguiSupport.SetupImgui();

        LogWindow logWindow = new LogWindow();
        _imguiSupport.AddWindow(logWindow, "Tools");

        _fileLogger = new FileLogger(_hooks, logWindow);
        _fileLogger.Init(_startupScanner);

        _gameStateHook = new GameStateHook(_hooks);
        _gameStateHook.Init(_startupScanner);

        var camPosOverlay = new GameOverlay(_gameStateHook);
        _imguiSupport.AddWindow(camPosOverlay);

        CreateImGuiWindows();
    }

    public void CreateImGuiWindows()
    {
        // Create windows
        CharacterManagerWindow characterManagerWindow = new();
        _imguiSupport.AddWindow(characterManagerWindow, "Managers");

        GemManagerWindow gemManagerWindow = new();
        _imguiSupport.AddWindow(gemManagerWindow, "Managers");

        ItemManagerWindow itemManagerWindow = new();
        _imguiSupport.AddWindow(itemManagerWindow, "Managers");

        LimitManagerWindow limitManagerWindow = new();
        _imguiSupport.AddWindow(limitManagerWindow, "Managers");

        SkillManagerWindow skillManagerWindow = new();
        _imguiSupport.AddWindow(skillManagerWindow, "Managers");

        WeaponManagerWindow weaponManagerWindow = new();
        _imguiSupport.AddWindow(weaponManagerWindow, "Managers");

        // Create hooks for windows
        _charManagerHook = new CharacterManagerHook(_hooks, characterManagerWindow);
        _charManagerHook.Init(_startupScanner);

        _gemManagerHook = new GemManagerHook(_hooks, gemManagerWindow);
        _gemManagerHook.Init(_startupScanner);

        _itemManagerHook = new ItemManagerHook(_hooks, itemManagerWindow);
        _itemManagerHook.Init(_startupScanner);

        _limitApManagerHook = new LimitApManagerHook(_hooks, limitManagerWindow);
        _limitApManagerHook.Init(_startupScanner);

        _skillManagerHook = new SkillManagerHook(_hooks, skillManagerWindow);
        _skillManagerHook.Init(_startupScanner);

        _weaponManagerHook = new WeaponManagerHook(_hooks, weaponManagerWindow);
        _weaponManagerHook.Init(_startupScanner);
    }


    #region Standard Overrides
    public override void ConfigurationUpdated(Config configuration)
    {
        // Apply settings from configuration.
        // ... your code here.
        _configuration = configuration;
        _logger.WriteLine($"[{_modConfig.ModId}] Config Updated: Applying");
    }
    #endregion

    #region For Exports, Serialization etc.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public Mod() { }
#pragma warning restore CS8618
    #endregion

}