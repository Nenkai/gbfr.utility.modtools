using Reloaded.Hooks.Definitions;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using IReloadedHooks = Reloaded.Hooks.ReloadedII.Interfaces.IReloadedHooks;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;
using gbfr.utility.modtools.Windows;


namespace gbfr.utility.modtools.Hooks;

public unsafe class FileLogger
{
    private IReloadedHooks _hooks;
    private LogWindow _logWindow;

    private delegate void OpenFile(FileOpenResult* result, uint a2, StringWrap* fileName);
    private IHook<OpenFile> _openFileHook;

    private delegate int FileExists(StringWrap* fileName);
    private IHook<FileExists> _fileExistsHook;

    private delegate void OpenFile2(IntPtr a1, StringWrap* fileName, IntPtr @params, IntPtr a4, IntPtr a5);
    private IHook<OpenFile2> _openFile2Hook;


    public FileLogger(IReloadedHooks hooks, LogWindow logWindow)
    {
        _hooks = hooks;
        _logWindow = logWindow;
    }

    public void Init(IStartupScanner startupScanner)
    {
        startupScanner.AddMainModuleScan("55 41 57 41 56 41 55 41 54 56 57 53 48 81 EC ?? ?? ?? ?? 48 8D AC 24 ?? ?? ?? ?? 48 C7 45 ?? ?? ?? ?? ?? 4C 89 C7 41 89 D6", e =>
        {
            if (!e.Found)
                return;

            var addr = Process.GetCurrentProcess().MainModule.BaseAddress + e.Offset;
            _openFileHook = _hooks.CreateHook<OpenFile>(OpenFileImpl, addr).Activate();
        });

        startupScanner.AddMainModuleScan("56 57 48 83 EC ?? 80 3D ?? ?? ?? ?? ?? 75", e =>
        {
            if (!e.Found)
                return;

            var addr = Process.GetCurrentProcess().MainModule.BaseAddress + e.Offset;
            _fileExistsHook = _hooks.CreateHook<FileExists>(FileExistsImpl, addr).Activate();
        });

        startupScanner.AddMainModuleScan("55 41 57 41 56 56 57 53 48 81 EC ?? ?? ?? ?? 48 8D AC 24 ?? ?? ?? ?? 48 C7 85 ?? ?? ?? ?? ?? ?? ?? ?? 4D 89 CE 48 89 D7", e =>
        {
            if (!e.Found)
                return;

            var addr = Process.GetCurrentProcess().MainModule.BaseAddress + e.Offset;
            _openFile2Hook = _hooks.CreateHook<OpenFile2>(OpenFile2Impl, addr).Activate();
        });
    }

    private void OpenFileImpl(FileOpenResult* result, uint a2, StringWrap* fileName)
    {
        _openFileHook.OriginalFunction(result, a2, fileName);

        if (fileName is not null)
        {
            string str = Marshal.PtrToStringAnsi((nint)fileName->pStr);

            if (result->pFileStorage is null)
                _logWindow.Log(nameof(FileLogger), $"open (not found): {str}");
            else
                _logWindow.Log(nameof(FileLogger), $"open (ok): {str}, size=0x{result->FileSize:X8}");
        }
        
    }

    private int FileExistsImpl(StringWrap* fileName)
    {
        var res = _fileExistsHook.OriginalFunction(fileName);

        if (fileName is not null && fileName->pStr is not null)
        {
            string str = Marshal.PtrToStringAnsi((nint)fileName->pStr);
            if (res == 0)
                _logWindow.Log(nameof(FileLogger), $"exists (not found): {str}");
            else
                _logWindow.Log(nameof(FileLogger), $"exists (ok): {str}");
        }
        
        return res;
    }

    private void OpenFile2Impl(IntPtr a1, StringWrap* fileName, IntPtr @params, IntPtr a4, IntPtr a5)
    {
        _openFile2Hook.OriginalFunction(a1, fileName, @params, a4, a5);

        if (fileName is not null && fileName->pStr is not null)
        {
            string str = Marshal.PtrToStringAnsi((nint)fileName->pStr);
            _logWindow.Log(nameof(FileLogger), $"open2: {str}");
        }
    }
}
