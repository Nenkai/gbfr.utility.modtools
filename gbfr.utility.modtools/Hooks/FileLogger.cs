using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;

using SharedScans.Interfaces;
using Reloaded.Mod.Interfaces;


namespace gbfr.utility.modtools.Hooks;

public unsafe class FileLogger
{
    private ISharedScans _scans;
    private ILogger _logger;

    public delegate void OpenFile(FileOpenResult* result, uint a2, StringWrap* fileName);
    public HookContainer<OpenFile> HOOK_OpenFile { get; private set; }

    public delegate int FileExists(StringWrap* fileName);
    private HookContainer<FileExists> HOOK_FileExists;

    public delegate void OpenFile2(IntPtr a1, StringWrap* fileName, IntPtr @params, IntPtr a4, IntPtr a5);
    private HookContainer<OpenFile2> HOOK_OpenFile2;

    public Dictionary<string, string> Patterns = new()
    {
        [nameof(OpenFile)] = "55 41 57 41 56 41 55 41 54 56 57 53 48 81 EC ?? ?? ?? ?? 48 8D AC 24 ?? ?? ?? ?? 48 C7 45 ?? ?? ?? ?? ?? 4C 89 C7 41 89 D6",
        [nameof(FileExists)] = "56 57 48 83 EC ?? 80 3D ?? ?? ?? ?? ?? 75",
        [nameof(OpenFile2)] = "55 41 57 41 56 56 57 53 48 81 EC ?? ?? ?? ?? 48 8D AC 24 ?? ?? ?? ?? 48 C7 85 ?? ?? ?? ?? ?? ?? ?? ?? 4D 89 CE 48 89 D7",
    };

    public FileLogger(ISharedScans scans, ILogger logger)
    {
        _scans = scans;
        _logger = logger;
    }

    public void Init()
    {
        foreach (var pattern in Patterns)
            _scans.AddScan(pattern.Key, pattern.Value);

        HOOK_OpenFile = _scans.CreateHook<OpenFile>(OpenFileImpl, "a");
        HOOK_FileExists = _scans.CreateHook<FileExists>(FileExistsImpl, "a");
        HOOK_OpenFile2 = _scans.CreateHook<OpenFile2>(OpenFile2Impl, "a");
    }

    private void OpenFileImpl(FileOpenResult* result, uint a2, StringWrap* fileName)
    {
        HOOK_OpenFile.Hook.OriginalFunction(result, a2, fileName);

        if (fileName is not null)
        {
            string str = Marshal.PtrToStringAnsi((nint)fileName->pStr);

            if (result->pFileStorage is null)
                _logger.WriteLine($"open (not found): {str}");
            else
                _logger.WriteLine($"open (ok): {str}, size=0x{result->FileSize:X8}");
        }
        
    }

    private int FileExistsImpl(StringWrap* fileName)
    {
        var res = HOOK_FileExists.Hook.OriginalFunction(fileName);

        if (fileName is not null && fileName->pStr is not null)
        {
            string str = Marshal.PtrToStringAnsi((nint)fileName->pStr);
            if (res == 0)
                _logger.WriteLine($"exists (not found): {str}");
            else
                _logger.WriteLine($"exists (ok): {str}");
        }
        
        return res;
    }

    private void OpenFile2Impl(IntPtr a1, StringWrap* fileName, IntPtr @params, IntPtr a4, IntPtr a5)
    {
        HOOK_OpenFile2.Hook.OriginalFunction(a1, fileName, @params, a4, a5);

        if (fileName is not null && fileName->pStr is not null)
        {
            string str = Marshal.PtrToStringAnsi((nint)fileName->pStr);
            _logger.WriteLine($"open2: {str}");
        }
    }
}
