using gbfr.utility.modtools.ImGuiSupport.Windows;
using gbfr.utility.modtools.Native;

using NenTools.ImGui.Interfaces;
using NenTools.ImGui.Interfaces.Shell;
using NenTools.ImGui.Native;

using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace gbfr.utility.modtools.ImGuiSupport;

public unsafe class ImGuiInputHookManager
{
    private readonly IImGuiShell _imGuiShell;
    private readonly IReloadedHooks _hooks;

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

    private RawMouseState _lastMouseState;

    public bool MouseActiveWhileMenuOpen = true;

    private bool _insertKeyWasDown;

    public ImGuiInputHookManager(IImGuiShell imGuiShell, IReloadedHooks hooks)
    {
        _imGuiShell = imGuiShell;
        _hooks = hooks;
    }

    /////////////////////////////
    // HOOKS
    /////////////////////////////

    public void SetupInputHooks()
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
        if (instance == _mouseDevice && ImGuiMethods.GetIO()->WantCaptureMouse ||
            instance == _keyboardDevice && ImGuiMethods.GetIO()->WantCaptureKeyboard) // ImGui wants input? don't forward to game
            return 0x8007000C; // DIERR_NOTACQUIRED

        var res = _getDeviceStateHook.OriginalFunction(instance, cbData, lpvData);

        if (instance == _mouseDevice)
        {
            if (_imGuiShell.ContextCreated && _imGuiShell.IsMainMenuOpen && !_imGuiShell.MouseActiveWhileMenuOpen)
                return 0x8007000C; // DIERR_NOTACQUIRED
        }
        else if (instance == _keyboardDevice)
        {
            // Insert key to show/hide ui
            bool insertIsDown = lpvData[(byte)DirectXKeyStrokes.DIK_INSERT] != 0;
            if (insertIsDown && !_insertKeyWasDown)
            {
                _imGuiShell.ToggleMenuState();
                ShowCursorImpl(_imGuiShell.IsMainMenuOpen);

                _insertKeyWasDown = insertIsDown;
                return 0x8007000C; // DIERR_NOTACQUIRED
            }

            _insertKeyWasDown = insertIsDown;
            return 0; // DI_OK
        }

        return res;
    }

    private int ShowCursorImpl(bool show)
    {
        // What the hell is ShowCursor man i just wanna disable or enable the cursor. What is this counter nonsense
        // I guess there's this
        // https://devblogs.microsoft.com/oldnewthing/20091217-00/?p=15643

        if (_imGuiShell.IsMainMenuOpen)
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
        if (_imGuiShell.IsMainMenuOpen)
            return false;

        return _setCursorPosHook.OriginalFunction(X, Y);
    }
}
