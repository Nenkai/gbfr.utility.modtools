using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;

using Reloaded.Mod.Interfaces;
using Reloaded.Hooks.Definitions;

using RyoTune.Reloaded;

using gbfr.utility.modtools.ImGuiSupport;

namespace gbfr.utility.modtools.Hooks;

public unsafe class FileLogger : IHookBase
{
    private ILogger _logger;

    public delegate void OpenFile(FileLoadResult* result, uint a2, StringWrap* fileName);
    public IHook<OpenFile> HOOK_OpenFile { get; private set; }

    public delegate int FileExists(StringWrap* fileName);
    private IHook<FileExists> HOOK_FileExists;

    public delegate void OpenFile2(IntPtr a1, StringWrap* fileName, IntPtr @params, IntPtr a4, IntPtr a5);
    private IHook<OpenFile2> HOOK_OpenFile2;

    public FileLogger(ILogger logger)
    {
        _logger = logger;
    }

    public void Init()
    {
        Project.Scans.AddScanHook(nameof(OpenFile), (result, hooks)
            => HOOK_OpenFile = hooks.CreateHook<OpenFile>(OpenFileImpl, result).Activate());
        Project.Scans.AddScanHook(nameof(FileExists), (result, hooks)
            => HOOK_FileExists = hooks.CreateHook<FileExists>(FileExistsImpl, result).Activate());
        Project.Scans.AddScanHook(nameof(OpenFile2), (result, hooks)
            => HOOK_OpenFile2 = hooks.CreateHook<OpenFile2>(OpenFile2Impl, result).Activate());
    }

    private void OpenFileImpl(FileLoadResult* result, uint a2, StringWrap* fileName)
    {
        HOOK_OpenFile.OriginalFunction(result, a2, fileName);

        if (fileName is not null)
        {
            string str = Marshal.PtrToStringAnsi((nint)fileName->pStr);

            if (ImGuiConfig.LogFiles)
            {
                if (result->ChunkFileStorage is null)
                    _logger.WriteLine($"open (not found): {str}");
                else
                    _logger.WriteLine($"open (ok): {str}, size=0x{result->FileSize:X8}");
            }
        }
        
    }

    private int FileExistsImpl(StringWrap* fileName)
    {
        var res = HOOK_FileExists.OriginalFunction(fileName);

        if (ImGuiConfig.LogFiles)
        {
            if (fileName is not null && fileName->pStr is not null)
            {
                string str = Marshal.PtrToStringAnsi((nint)fileName->pStr);
                if (res == 0)
                    _logger.WriteLine($"exists (not found): {str}");
                else
                    _logger.WriteLine($"exists (ok): {str}");
            }
        }
        
        return res;
    }

    private void OpenFile2Impl(IntPtr a1, StringWrap* fileName, IntPtr @params, IntPtr a4, IntPtr a5)
    {
        HOOK_OpenFile2.OriginalFunction(a1, fileName, @params, a4, a5);

        if (ImGuiConfig.LogFiles)
        {
            if (fileName is not null && fileName->pStr is not null)
            {
                string str = Marshal.PtrToStringAnsi((nint)fileName->pStr);
                _logger.WriteLine($"open2: {str}");
            }
        }
    }
}
