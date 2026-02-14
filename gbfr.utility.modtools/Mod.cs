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
using gbfr.utility.modtools.Hooks.Fsm;
using Microsoft.Extensions.DependencyInjection;

using RyoTune.Reloaded;

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

    private ImguiSupport _imguiSupport;

    private IServiceProvider _services;

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

        CreateServices();
        Project.Initialize(_modConfig, _modLoader, _logger);

        _imguiSupport.SetupImgui(_modLoader.GetDirectoryForModId(_modConfig.ModId));

        foreach (var svc in _services.GetServices<IHookBase>())
            svc.Init();

        CreateImGuiWindows();
    }

    private void CreateServices()
    {
        _imguiSupport = new ImguiSupport(_hooks);

        _services = new ServiceCollection()
            .AddSingleton(_logger)
            .AddSingleton(_imguiSupport)
            // Hooks
            .AddSingletonAs<IHookBase, GameStateHook>()
            .AddSingletonAs<IHookBase, ReflectionHooks>()
            .AddSingletonAs<IHookBase, FileLogger>()
            .AddSingletonAs<IHookBase, EffectDataHooks>()
            .AddSingletonAs<IHookBase, EventHooks>()
            .AddSingletonAs<IHookBase, EntityHooks>()

            // Hooks (tables)
            .AddSingletonAs<IHookBase, CharacterManagerHook>()
            .AddSingletonAs<IHookBase, GemManagerHook>()
            .AddSingletonAs<IHookBase, ItemManagerHook>()
            .AddSingletonAs<IHookBase, LimitApManagerHook>()
            .AddSingletonAs<IHookBase, SkillManagerHook>()
            .AddSingletonAs<IHookBase, WeaponManagerHook>()

            // ImGui
            .AddSingleton<LogWindow>()
            .AddSingleton<CharacterManagerWindow>()
            .AddSingleton<GemManagerWindow>()
            .AddSingleton<ItemManagerWindow>()
            .AddSingleton<LimitManagerWindow>()
            .AddSingleton<SkillManagerWindow>()
            .AddSingleton<WeaponManagerWindow>()

            .AddSingleton<EffectEditWindow>()

            .AddSingleton<EntitiesWindow>()

            .AddSingleton<GameOverlay>()

            .AddSingleton<DumpMenuButton>()
            .AddSingleton<MouseControlButton>()
            .BuildServiceProvider();
    }

    public void CreateImGuiWindows()
    {
        _imguiSupport.AddWindow(_services.GetRequiredService<LogWindow>(), "Tools");

        // Main menu stuff
        _imguiSupport.AddComponent("Tools", _services.GetRequiredService<DumpMenuButton>());

        // Create windows
        _imguiSupport.AddWindow(_services.GetRequiredService<CharacterManagerWindow>(), "Managers");
        _imguiSupport.AddWindow(_services.GetRequiredService<GemManagerWindow>(), "Managers");
        _imguiSupport.AddWindow(_services.GetRequiredService<ItemManagerWindow>(), "Managers");
        _imguiSupport.AddWindow(_services.GetRequiredService<LimitManagerWindow>(), "Managers");
        _imguiSupport.AddWindow(_services.GetRequiredService<SkillManagerWindow>(), "Managers");
        _imguiSupport.AddWindow(_services.GetRequiredService<WeaponManagerWindow>(), "Managers");

        _imguiSupport.AddWindow(_services.GetRequiredService<EffectEditWindow>(), "Effects");

        _imguiSupport.AddWindow(_services.GetRequiredService<EntitiesWindow>(), "Entities");

        _imguiSupport.AddWindow(_services.GetRequiredService<GameOverlay>(), "Other");

        _imguiSupport.AddWindow(OverlayLogger.Instance);

        _imguiSupport.AddComponent("Other", _services.GetRequiredService<MouseControlButton>());
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