using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

using DearImguiSharp;

using Reloaded.Hooks.Definitions;
using Reloaded.Imgui.Hook.Direct3D11;
using Reloaded.Imgui.Hook.Implementations;
using Reloaded.Imgui.Hook;

using gbfr.utility.modtools.ImGuiSupport.Windows;
using Microsoft.VisualBasic;
using gbfr.utility.modtools.Native;
using System.Web;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace gbfr.utility.modtools.ImGuiSupport;

public unsafe class ImguiSupport
{
    public delegate int ShowCursor(bool show);
    private IHook<ShowCursor> _showCursorHook;

    public delegate bool SetCursorPos(int X, int Y);
    private IHook<SetCursorPos> _setCursorPosHook;

    public delegate nint DirectInput8Create(nint hinst, int dwVersion, nint riidltf, nint ppvOut, nint punkOuter);
    private IHook<DirectInput8Create> _directInputCreateHook;

    public delegate nint CreateDevice(nint instance, Guid rguid, nint lplpDirectInputDevice, nint pUnkOuter);
    private IHook<CreateDevice> _createDeviceHook;

    public delegate nuint GetDeviceState(nint instance, int cbData, byte* lpvData);
    private IHook<GetDeviceState> _getDeviceStateHook;

    private bool _menuVisible = true;
    private byte _lastInsertState;
    private RawMouseState _lastMouseState;

    private readonly IReloadedHooks? _hooks;

    private List<IImguiWindow> _windows = [];
    private readonly Dictionary<string, List<IImguiMenuComponent>> _menuCategoryToComponentList = [];

    public bool MouseActiveWhileMenuOpen = true;

    public ImguiSupport(IReloadedHooks hooks)
    {
        _hooks = hooks;
    }

    public void SetupImgui(string modFolder)
    {
        SetupInputHooks();

        SDK.Init(_hooks);
        ImguiHook.Create(Render, new ImguiHookOptions()
        {
            EnableViewports = true, // Enable docking.
            IgnoreWindowUnactivate = true, // May help if game pauses when it loses focus.
            Implementations = [new ImguiHookDx11()],
        });

        ConfigureImgui(modFolder);
    }

    private static void ConfigureImgui(string modFolder)
    {
        var io = ImGui.GetIO();

        string fontPath = Path.Combine(modFolder, "Fonts", "Roboto", "Roboto-Medium.ttf");
        ImGui.ImFontAtlasAddFontFromFileTTF(io.Fonts, fontPath, 15.0f, null, ref Utils.NullReference<ushort>());

        var style = ImGui.GetStyle();
        style.FrameRounding = 4.0f;
        style.WindowRounding = 4.0f;
        style.WindowBorderSize = 0.0f;
        style.PopupBorderSize = 0.0f;
        style.GrabRounding = 4.0f;
    }

    public void AddMenuSeparator(string category)
    {
        AddComponent(category, new ImguiSeparator());
    }

    public void AddWindow(IImguiWindow window, string mainMenuCategory = null)
    {
        _windows.Add(window);

        if (!string.IsNullOrEmpty(mainMenuCategory))
            AddComponent(mainMenuCategory, window);
    }

    public void AddComponent(string category, IImguiMenuComponent component)
    {
        if (!_menuCategoryToComponentList.TryGetValue(category, out List<IImguiMenuComponent> imguiMenuComponents))
            _menuCategoryToComponentList.TryAdd(category, [component]);
        else
            imguiMenuComponents.Add(component);
    }

    public void Render()
    {
        foreach (IImguiWindow window in _windows)
        {
            if (window.IsOverlay)
                window.Render();
        }

        if (!_menuVisible)
            return;

        if (ImGui.BeginMainMenuBar())
        {
            foreach (var mainMenuCategory in _menuCategoryToComponentList)
            {
                if (ImGui.BeginMenu(mainMenuCategory.Key, true))
                {
                    foreach (IImguiMenuComponent component in mainMenuCategory.Value)
                    {
                        component.BeginMenuComponent();
                    }

                    ImGui.EndMenu();
                }
            }

            ImGui.EndMainMenuBar();
        }

        foreach (var window in _windows)
        {
            if (!window.IsOverlay)
                window.Render();
        }
    }


    /////////////////////////////
    // HOOKS
    /////////////////////////////

    private void SetupInputHooks()
    {
        // Hook cursor visibility as the game hides it
        nint kernel32Handle = NativeMethods.LoadLibraryW("user32");
        nint showCursorPtr = NativeMethods.GetProcAddress(kernel32Handle, "ShowCursor");
        _showCursorHook = _hooks.CreateHook<ShowCursor>(ShowCursorImpl, showCursorPtr).Activate();

        // Hook cursor position as the game sets it to center of the screen otherwise?
        nint setCursorPosPtr = NativeMethods.GetProcAddress(kernel32Handle, "SetCursorPos");
        _setCursorPosHook = _hooks.CreateHook<SetCursorPos>(SetCursorPosImpl, setCursorPosPtr).Activate();

        // Chain hook direct input so imgui inputs don't also get passed to the game.
        var handle = NativeMethods.GetModuleHandle("dinput8.dll");
        nint directInput8CreatePtr = NativeMethods.GetProcAddress(handle, "DirectInput8Create");
        _directInputCreateHook = _hooks.CreateHook<DirectInput8Create>(DirectInput8CreateImpl, directInput8CreatePtr).Activate();
    }

    private nint DirectInput8CreateImpl(nint hinst, int dwVersion, nint riidltf, nint ppvOut, nint punkOuter)
    {
        nint result = _directInputCreateHook.OriginalFunction(hinst, dwVersion, riidltf, ppvOut, punkOuter);

        // Get location of IDirectInput8::CreateDevice and hook it
        long* instancePtr = (long*)*(long*)ppvOut;
        long** vtbl = (long**)*instancePtr;
        nint createDevicePtr = (nint)vtbl[3];

        _createDeviceHook = _hooks.CreateHook<CreateDevice>(CreateDeviceImpl, createDevicePtr).Activate();
        return result;
    }

    private nint _mouseDevice;
    private nint _keyboardDevice;
    private nint CreateDeviceImpl(nint /* this */ instance, Guid rguid, nint lplpDirectInputDevice, nint pUnkOuter)
    {
        if (_directInputCreateHook.IsHookEnabled)
            _directInputCreateHook.Disable();

        nint result = _createDeviceHook.OriginalFunction(instance, rguid, lplpDirectInputDevice, pUnkOuter);
        long* instancePtr = (long*)*(long*)lplpDirectInputDevice;

        if (_getDeviceStateHook is null)
        {
            // Get location of IDirectInputDevice8::GetDeviceState and hook it
            long** vtbl = (long**)*instancePtr;

            nint getDeviceStatePtr = (nint)vtbl[9];
            _getDeviceStateHook = _hooks.CreateHook<GetDeviceState>(GetDeviceStateImpl, getDeviceStatePtr).Activate();
        }

        if (rguid == NativeConstants.SysMouseGuid)
        {
            _mouseDevice = (nint)instancePtr;
        }
        else if (rguid == NativeConstants.SysKeyboardGuid)
        {
            _keyboardDevice = (nint)instancePtr;
        }

        return result;
    }

    private nuint GetDeviceStateImpl(nint instance, int cbData, byte* lpvData)
    {
        var io = ImGui.GetIO();
        if (instance == _mouseDevice && io.WantCaptureMouse ||
            instance == _keyboardDevice && io.WantCaptureKeyboard) // ImGui wants input? don't forward to game
            return 0x8007001E; // DIERR_LOSTINPUT

        var res = _getDeviceStateHook.OriginalFunction(instance, cbData, lpvData);

        if (instance == _mouseDevice)
        {
            if (_menuVisible && !MouseActiveWhileMenuOpen)
            {
                return 0x8007001E; // DIERR_LOSTINPUT

                /*
                // If the menu is visible but outside imgui, then don't apply mouse move changes
                RawMouseState mouseState = *(RawMouseState*)lpvData;
                mouseState.X = 0;
                mouseState.Y = 0;
                mouseState.Z = 0;
                */
            }
        }
        else if (instance == _keyboardDevice)
        {
            // Insert key to show/hide ui
            var currentInsertState = lpvData[(byte)DirectXKeyStrokes.DIK_INSERT];
            if (currentInsertState != 0 && _lastInsertState != currentInsertState)
            {
                _menuVisible = !_menuVisible;
                ShowCursorImpl(_menuVisible);
            }

            _lastInsertState = currentInsertState;
            return 0; // DI_OK
        }

        return res;
    }

    private int ShowCursorImpl(bool show)
    {
        // What the hell is ShowCursor man i just wanna disable or enable the cursor. What is this counter nonsense
        // I guess there's this
        // https://devblogs.microsoft.com/oldnewthing/20091217-00/?p=15643

        if (_menuVisible)
        {
            int cnt = 0;
            do
            {
                cnt = _showCursorHook.OriginalFunction(true);
            }
            while (cnt < 1);
            return cnt;
        }
        else
        {
            int cnt = 0;
            do
            {
                cnt = _showCursorHook.OriginalFunction(false);
            }
            while (cnt > -1);
            return cnt;
        }
    }

    private bool SetCursorPosImpl(int X, int Y)
    {
        if (_menuVisible)
            return false;

        return _setCursorPosHook.OriginalFunction(X, Y);
    }
}
