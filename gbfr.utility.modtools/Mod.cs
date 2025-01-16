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
using gbfr.utility.modtools.ImGuiSupport.Windows;
using gbfr.utility.modtools.ImGuiSupport.MenuButtons;
using gbfr.utility.modtools.ImGuiSupport.Windows.Tables;
using gbfr.utility.modtools.ImGuiSupport;
using SharedScans.Interfaces;
using gbfr.utility.modtools.Hooks.Managers;
using gbfr.utility.modtools.Hooks.Effects;
using gbfr.utility.modtools.Hooks.Reflection;

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
    private static ISharedScans? _sharedScans = null!;

    private ImguiSupport _imguiSupport;

    private FileLogger _fileLogger;
    private ReflectionHooks _reflectionHooks;

    private GameStateHook _gameStateHook;

    private CharacterManagerHook _charManagerHook;
    private GemManagerHook _gemManagerHook;
    private ItemManagerHook _itemManagerHook;
    private LimitApManagerHook _limitApManagerHook;
    private SkillManagerHook _skillManagerHook;
    private WeaponManagerHook _weaponManagerHook;

    private EffectDataHooks _effectDataHooks;

    public Mod(ModContext context)
    {
        _modLoader = context.ModLoader;
        _hooks = context.Hooks;
        _logger = context.Logger;
        _owner = context.Owner;
        _configuration = context.Configuration;
        _modConfig = context.ModConfig;

#if DEBUG
        Debugger.Launch();
#endif

        var startupScannerController = _modLoader.GetController<IStartupScanner>();
        if (startupScannerController == null || !startupScannerController.TryGetTarget(out _startupScanner))
        {
            return;
        }

        var sharedScansController = _modLoader.GetController<ISharedScans>();
        if (sharedScansController == null || !sharedScansController.TryGetTarget(out _sharedScans))
        {
            _logger.WriteLine($"[{_modConfig.ModId}] Unable to get ISharedScans. Framework will not load!");
            return;
        }

        _imguiSupport = new ImguiSupport(_hooks);
        _imguiSupport.SetupImgui(_modLoader.GetDirectoryForModId(_modConfig.ModId));

        CreateHooks();
        CreateImGuiWindows();
    }

    private void CreateHooks()
    {
        _gameStateHook = new GameStateHook(_hooks);
        _gameStateHook.Init(_startupScanner);

        _reflectionHooks = new ReflectionHooks(_sharedScans, _logger);
        _reflectionHooks.Init();

        //_fileLogger = new FileLogger(_sharedScans, _logger);
        //_fileLogger.Init();

        _effectDataHooks = new EffectDataHooks(_sharedScans);
        _effectDataHooks.Init();

        // Create hooks for windows
        _charManagerHook = new CharacterManagerHook(_sharedScans);
        _charManagerHook.Init();

        _gemManagerHook = new GemManagerHook(_sharedScans);
        _gemManagerHook.Init();

        _itemManagerHook = new ItemManagerHook(_sharedScans);
        _itemManagerHook.Init();

        _limitApManagerHook = new LimitApManagerHook(_hooks);
        _limitApManagerHook.Init(_startupScanner);

        _skillManagerHook = new SkillManagerHook(_hooks);
        _skillManagerHook.Init(_startupScanner);

        _weaponManagerHook = new WeaponManagerHook(_hooks);
        _weaponManagerHook.Init(_startupScanner);
    }

    public void CreateImGuiWindows()
    {
        LogWindow logWindow = new LogWindow(_logger);
        _imguiSupport.AddWindow(logWindow, "Tools");

        // Main menu stuff
        _imguiSupport.AddComponent("Tools", new DumpMenuButton(_reflectionHooks));

        // Create windows
        CharacterManagerWindow characterManagerWindow = new(_charManagerHook);
        _imguiSupport.AddWindow(characterManagerWindow, "Managers");

        GemManagerWindow gemManagerWindow = new(_gemManagerHook);
        _imguiSupport.AddWindow(gemManagerWindow, "Managers");

        ItemManagerWindow itemManagerWindow = new(_itemManagerHook);
        _imguiSupport.AddWindow(itemManagerWindow, "Managers");

        LimitManagerWindow limitManagerWindow = new(_limitApManagerHook);
        _imguiSupport.AddWindow(limitManagerWindow, "Managers");

        SkillManagerWindow skillManagerWindow = new(_skillManagerHook);
        _imguiSupport.AddWindow(skillManagerWindow, "Managers");

        WeaponManagerWindow weaponManagerWindow = new(_weaponManagerHook);
        _imguiSupport.AddWindow(weaponManagerWindow, "Managers");

        EffectEditWindow effectEditWindow = new EffectEditWindow(_effectDataHooks);
        _imguiSupport.AddWindow(effectEditWindow, "Effects");

        var camPosOverlay = new GameOverlay(_gameStateHook);
        _imguiSupport.AddWindow(camPosOverlay, "Other");

        _imguiSupport.AddComponent("Other", new MouseControlButton(_imguiSupport));
        _imguiSupport.AddMenuSeparator("Other");
        _imguiSupport.AddWindow(new DemoWindow(), "Other");
        _imguiSupport.AddWindow(new AboutWindow(_modConfig), "Other");
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