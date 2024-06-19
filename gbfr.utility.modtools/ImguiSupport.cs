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

namespace gbfr.utility.modtools;

public unsafe class ImguiSupport
{
    public delegate int ShowCursor(bool show);
    private IHook<ShowCursor> _showCursorHook;

    public delegate bool SetCursorPos(int X, int Y);
    private IHook<SetCursorPos> _setCursorPosHook;

    public delegate IntPtr DirectInput8Create(IntPtr hinst, int dwVersion, IntPtr riidltf, IntPtr ppvOut, IntPtr punkOuter);
    private IHook<DirectInput8Create> _directInputCreateHook;

    public delegate IntPtr CreateDevice(IntPtr instance, Guid rguid, IntPtr lplpDirectInputDevice, IntPtr pUnkOuter);
    private IHook<CreateDevice> _createDeviceHook;

    public delegate UIntPtr GetDeviceState(IntPtr instance, int cbData, byte* lpvData);
    private IHook<GetDeviceState> _getDeviceStateHook;

    public static readonly Guid SysMouse = new Guid("6f1d2b60-d5a0-11cf-bfc7-444553540000");
    public static readonly Guid SysKeyboard = new Guid("6f1d2b61-d5a0-11cf-bfc7-444553540000");

    private bool _menuVisible = true;
    private byte _lastInsertState;
    private RawMouseState _lastMouseState;

    private readonly IReloadedHooks? _hooks;

    public ImguiSupport(IReloadedHooks hooks)
    {
        _hooks = hooks;
    }

    public void SetupImgui()
    {
        // Hook cursor visibility as the game hides it
        IntPtr kernel32Handle = NativeMethods.LoadLibraryW("user32");
        IntPtr showCursorPtr = NativeMethods.GetProcAddress(kernel32Handle, "ShowCursor");
        _showCursorHook = _hooks.CreateHook<ShowCursor>(ShowCursorImpl, showCursorPtr).Activate();

        // Hook cursor position as the game sets it to center of the screen otherwise?
        IntPtr setCursorPosPtr = NativeMethods.GetProcAddress(kernel32Handle, "SetCursorPos");
        _setCursorPosHook = _hooks.CreateHook<SetCursorPos>(SetCursorPosImpl, setCursorPosPtr).Activate();

        // Chain hook direct input so imgui inputs don't also get passed to the game.
        var handle = NativeMethods.GetModuleHandle("dinput8.dll");
        IntPtr directInput8CreatePtr = NativeMethods.GetProcAddress(handle, "DirectInput8Create");
        _directInputCreateHook = _hooks.CreateHook<DirectInput8Create>(DirectInput8CreateImpl, directInput8CreatePtr).Activate();

        SDK.Init(_hooks);
        ImguiHook.Create(Render, new ImguiHookOptions()
        {
            EnableViewports = true, // Enable docking.
            IgnoreWindowUnactivate = true, // May help if game pauses when it loses focus.
            Implementations = new List<IImguiHook>() { new ImguiHookDx11() },
        });
    }

    public void Render()
    {
        if (!_menuVisible)
            return;

        if (ImGui.BeginMainMenuBar())
        {
            if (ImGui.BeginMenu("Tools", true))
            {
                if (ImGui.MenuItemEx("Logs", "", "", false, true))
                    LogWindow._isOpen = true;

                ImGui.EndMenu();
            }
            ImGui.EndMainMenuBar();
        }

        LogWindow.Render();
    }

    /////////////////////////////
    // HOOKS
    /////////////////////////////

    private IntPtr DirectInput8CreateImpl(IntPtr hinst, int dwVersion, IntPtr riidltf, IntPtr ppvOut, IntPtr punkOuter)
    {
        IntPtr result = _directInputCreateHook.OriginalFunction(hinst, dwVersion, riidltf, ppvOut, punkOuter);

        // Get location of IDirectInput8::CreateDevice and hook it
        long* instancePtr = (long*)*(long*)ppvOut;
        long** vtbl = (long**)*instancePtr;
        nint createDevicePtr = (nint)vtbl[3];

        _createDeviceHook = _hooks.CreateHook<CreateDevice>(CreateDeviceImpl, createDevicePtr).Activate();
        return result;
    }

    private IntPtr _mouseDevice;
    private IntPtr _keyboardDevice;
    private IntPtr CreateDeviceImpl(IntPtr /* this */ instance, Guid rguid, IntPtr lplpDirectInputDevice, IntPtr pUnkOuter)
    {
        if (_directInputCreateHook.IsHookEnabled)
            _directInputCreateHook.Disable();

        IntPtr result = _createDeviceHook.OriginalFunction(instance, rguid, lplpDirectInputDevice, pUnkOuter);
        long* instancePtr = (long*)*(long*)lplpDirectInputDevice;

        if (_getDeviceStateHook is null)
        {
            // Get location of IDirectInputDevice8::GetDeviceState and hook it
            long** vtbl = (long**)*instancePtr;

            nint getDeviceStatePtr = (nint)vtbl[9];
            _getDeviceStateHook = _hooks.CreateHook<GetDeviceState>(GetDeviceStateImpl, getDeviceStatePtr).Activate();
        }

        if (rguid == SysMouse)
        {
            _mouseDevice = (IntPtr)instancePtr;
        }
        else if (rguid == SysKeyboard)
        {
            _keyboardDevice = (IntPtr)instancePtr;
        }

        return result;
    }

    private UIntPtr GetDeviceStateImpl(IntPtr instance, int cbData, byte* lpvData)
    {
        var io = ImGui.GetIO();
        if (instance == _mouseDevice && io.WantCaptureMouse ||
            instance == _keyboardDevice && io.WantCaptureKeyboard) // ImGui wants input? don't forward to game
            return 0; // DI_OK

        var res = _getDeviceStateHook.OriginalFunction(instance, cbData, lpvData);

        if (instance == _mouseDevice)
        {
            if (_menuVisible)
            {
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
        if (_menuVisible)
            return _showCursorHook.OriginalFunction(true);

        return _showCursorHook.OriginalFunction(show);
    }

    private bool SetCursorPosImpl(int X, int Y)
    {
        if (_menuVisible)
            return false;

        return _setCursorPosHook.OriginalFunction(X, Y);
    }
}
