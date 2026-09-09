using gbfr.utility.modtools.Configuration;
using gbfr.utility.modtools.Hooks;
using gbfr.utility.modtools.Hooks.Behavior;
using gbfr.utility.modtools.Hooks.Effects;
using gbfr.utility.modtools.Hooks.Events;
using gbfr.utility.modtools.Hooks.Fsm;
using gbfr.utility.modtools.Hooks.Reflection;
using gbfr.utility.modtools.Hooks.Tables;
using gbfr.utility.modtools.ImGuiSupport;
using gbfr.utility.modtools.ImGuiSupport.MenuButtons;
using gbfr.utility.modtools.ImGuiSupport.Windows;
using gbfr.utility.modtools.ImGuiSupport.Windows.Tables;
using gbfr.utility.modtools.Template;

using gbfrelink.utility.manager.Interfaces;

using Microsoft.Extensions.DependencyInjection;

using NenTools.ImGui.Hooks.DirectX11;
using NenTools.ImGui.Implementation;
using NenTools.ImGui.Interfaces;
using NenTools.ImGui.Interfaces.Backend;
using NenTools.ImGui.Interfaces.Shell;
using NenTools.ImGui.Interfaces.Shell.Fonts;
using NenTools.ImGui.Shell;
using NenTools.ImGui.Shell.Windows;
using NenTools.Reloaded.ScanManager.Interfaces;

using Reloaded.Hooks.Definitions;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;

using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using Tomlyn;

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

    private IServiceProvider _services;
    private readonly IUserDefinedParams _userDefinedParams;
    private readonly IScanManager _scanManager;

    private readonly IBackendHook _backendHook;
    private IImGuiShell _imGuiShell;
    private ImGuiShellConfig _imGuiShellConfig;
    private IImGui _imGui;

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
        var userDefinedParamsRef = _modLoader.GetController<IUserDefinedParams>();
        if (userDefinedParamsRef == null || !userDefinedParamsRef.TryGetTarget(out _userDefinedParams))
        {
            _logger.WriteLine($"[{_modConfig.ModId}] IUserDefinedParams not found?  Rich presence will not load!", System.Drawing.Color.Red);
            return;
        }

        var scanManagerRef = _modLoader.GetController<IScanManager>();
        if (scanManagerRef == null || !scanManagerRef.TryGetTarget(out _scanManager))
        {
            _logger.WriteLine($"[{_modConfig.ModId}] IScanManager not found?  Rich presence will not load!", System.Drawing.Color.Red);
            return;
        }
        _scanManager.InitializeScans(Path.Combine(_modLoader.GetDirectoryForModId(_modConfig.ModId), "Signatures"), _modConfig.ModId);

        string signatureGroup = _userDefinedParams.IsEndlessRagnarok() ? "granblue_fantasy_relink_er" : "granblue_fantasy_relink";

        CreateServices();

        foreach (var svc in _services.GetServices<IHookBase>())
            svc.Init(signatureGroup);

        InitImGui();
    }

    private void CreateServices()
    {
        string shellConfigPath = Path.Combine(_modLoader.GetDirectoryForModId(_modConfig.ModId), "shell_config.toml");
        if (File.Exists(shellConfigPath))
        {
            try
            {
                _imGuiShellConfig = Toml.ToModel<ImGuiShellConfig>(File.ReadAllText(shellConfigPath));
                _imGuiShellConfig.SetPath(shellConfigPath);
            }
            catch (Exception ex)
            {
                _logger.WriteLine($"[{_modConfig.ModId}] Failed to load ImGui Shell config from '{shellConfigPath}' - {ex.Message}");
                _logger.WriteLine("Creating default config...");
                _imGuiShellConfig = new ImGuiShellConfig(shellConfigPath);
                _imGuiShellConfig.Save();
            }
        }
        else
        {
            _imGuiShellConfig = new ImGuiShellConfig(shellConfigPath);
            _imGuiShellConfig.Save();
        }

        _services = new ServiceCollection()
            .AddSingleton<ILogger>(_logger)
            .AddSingleton<IModConfig>(_modConfig)
            .AddSingleton<ImGuiShellConfig>(_imGuiShellConfig)
            .AddSingleton<IImGui, ImGui>()
            .AddSingleton<IBackendHook, DX11BackendHook>()
            .AddSingleton<IImGuiShell, ImGuiShell>()
            .AddSingleton<ImGuiInputHookManager>()
            .AddSingleton<IReloadedHooks>(_hooks)
            .AddSingleton<IScanManager>(_scanManager)
            .AddSingletonAs<IHookBase, GameStateHook>()
            .AddSingletonAs<IHookBase, ReflectionHooks>()
            .AddSingletonAs<IHookBase, FileLogger>()
            .AddSingletonAs<IHookBase, EffectDataHooks>()
            .AddSingletonAs<IHookBase, EventHooks>()
            .AddSingletonAs<IHookBase, EntityHooks>()
            .AddSingletonAs<IHookBase, TeleportHooks>()
            .AddSingletonAs<IHookBase, BehaviorFactoryHooks>()
            .AddSingletonAs<IHookBase, DebugPrintActionHook>()

            // Hooks (tables)
            /*
            .AddSingletonAs<IHookBase, CharacterManagerHook>()
            .AddSingletonAs<IHookBase, GemManagerHook>()
            .AddSingletonAs<IHookBase, ItemManagerHook>()
            .AddSingletonAs<IHookBase, LimitApManagerHook>()
            .AddSingletonAs<IHookBase, SkillManagerHook>()
            .AddSingletonAs<IHookBase, WeaponManagerHook>()
            */

            // ImGui
            /*
            .AddSingleton<CharacterManagerWindow>()
            .AddSingleton<GemManagerWindow>()
            .AddSingleton<ItemManagerWindow>()
            .AddSingleton<LimitManagerWindow>()
            .AddSingleton<SkillManagerWindow>()
            .AddSingleton<WeaponManagerWindow>()
            */
            //.AddSingleton<EffectEditWindow>()
            .AddSingleton<AboutWindow>()
            .AddSingleton<LogWindow>()
            .AddSingleton<EntitiesWindow>()
            .AddSingleton<TeleportPhaseEditWindow>()

            .AddSingleton<GameOverlay>()

            .AddSingleton<DumpMenuButton>()
            .AddSingleton<MouseControlButton>()
            .BuildServiceProvider();
    }


    private void InitImGui()
    {
        _imGui = _services.GetRequiredService<IImGui>();
        _imGuiShell = _services.GetRequiredService<IImGuiShell>();

        _imGuiShell.OnImGuiConfiguration += ConfigureImgui;
        _imGuiShell.OnLogMessage += (message, color) => _logger.WriteLine(message, color ?? System.Drawing.Color.White);
        _imGuiShell.OnFirstRender += () => _imGuiShell.LogWriteLine(_modConfig.ModId, "Relink Mod Tools initialized. Press INSERT to bring up the main menu.");
        var inputHook = _services.GetRequiredService<ImGuiInputHookManager>();
        inputHook.SetupInputHooks();

        _imGuiShell.Start(new BackendHookOptions()
        {
            // TODO: Implement viewports
            // EnableViewports = false, Not yet functional with DX12 hooks, it creates a new swapchain that shouldn't be hooked.
            Implementations = [.. _services.GetServices<IBackendHook>()]
        }).GetAwaiter().GetResult();

        CreateImGuiWindows();

        _imGuiShell.EnableOverlay();

        _modLoader.AddOrReplaceController<IImGui>(_owner, _imGui);
        _modLoader.AddOrReplaceController<IImGuiShell>(_owner, _imGuiShell);
    }

    private void ConfigureImgui()
    {
        string modFolder = _modLoader.GetDirectoryForModId(_modConfig.ModId);

        IImGuiIO io = _imGui.GetIO();
        io.ConfigFlags |= ImGuiConfigFlags.ImGuiConfigFlags_DockingEnable;

        // English font
        string robotoFontPath = Path.Combine(modFolder, "Fonts", "Roboto", "Roboto-Medium.ttf");
        _imGuiShell.FontManager.AddFontTTF(_modConfig.ModId, "Roboto-Medium", robotoFontPath, 15.0f, _imGui.ImFontAtlas_GetGlyphRangesDefault(io.Fonts), null!);

        // Japanese font
        string netoSansJpFontPath = Path.Combine(modFolder, "Fonts", "Noto", "NotoSansJP-Medium.ttf");
        _imGuiShell.FontManager.AddFontTTF(_modConfig.ModId, "NotoSansJP-Medium", netoSansJpFontPath, 17.0f, _imGui.ImFontAtlas_GetGlyphRangesJapanese(io.Fonts), new ImFontOptions { MergeMode = true });

        // Emojis
        string twitterColorEmojiFontPath = Path.Combine(modFolder, "Fonts", "TwitterColorEmoji", "twemoji.ttf");
        _imGuiShell.FontManager.AddFontTTF(_modConfig.ModId, "twemoji", twitterColorEmojiFontPath, 14.0f, [0x1, 0x1FFFF], new ImFontOptions()
        {
            FontLoaderFlags = (uint)(ImGuiFreeTypeLoaderFlags.ImGuiFreeTypeBuilderFlags_LoadColor | ImGuiFreeTypeLoaderFlags.ImGuiFreeTypeBuilderFlags_Bitmap),
            MergeMode = true
        });

        var style = _imGui.GetStyle();
        style.FrameRounding = 4.0f;
        style.WindowRounding = 4.0f;
        style.WindowBorderSize = 0.0f;
        style.PopupBorderSize = 0.0f;
        style.GrabRounding = 4.0f;
    }

    public void CreateImGuiWindows()
    {
        // Main menu stuff
        _imGuiShell.AddComponent(_services.GetRequiredService<LogWindow>(), "File");
        _imGuiShell.AddComponent(_services.GetRequiredService<DumpMenuButton>(), "File");

        // Create windows
        /*
        _imguiSupport.AddWindow(_services.GetRequiredService<CharacterManagerWindow>(), "Managers");
        _imguiSupport.AddWindow(_services.GetRequiredService<GemManagerWindow>(), "Managers");
        _imguiSupport.AddWindow(_services.GetRequiredService<ItemManagerWindow>(), "Managers");
        _imguiSupport.AddWindow(_services.GetRequiredService<LimitManagerWindow>(), "Managers");
        _imguiSupport.AddWindow(_services.GetRequiredService<SkillManagerWindow>(), "Managers");
        _imguiSupport.AddWindow(_services.GetRequiredService<WeaponManagerWindow>(), "Managers");

        _imguiSupport.AddWindow(_services.GetRequiredService<EffectEditWindow>(), "Effects");
        */


        _imGuiShell.AddComponent(_services.GetRequiredService<EntitiesWindow>(), "Mods");
        _imGuiShell.AddComponent(_services.GetRequiredService<TeleportPhaseEditWindow>(), "Mods");

        _imGuiShell.AddComponent(_services.GetRequiredService<GameOverlay>(), "Other");

        _imGuiShell.AddComponent(_services.GetRequiredService<MouseControlButton>(), "Other");
        _imGuiShell.AddComponent(_services.GetRequiredService<AboutWindow>(), "Other");
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