using gbfr.utility.modtools.ImGuiSupport;

using NenTools.Reloaded.ScanManager.Interfaces;

using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces;
using Reloaded.Hooks.Definitions.Enums;
using Reloaded.Memory.Interfaces;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace gbfr.utility.modtools.Hooks;

public unsafe class FileLogger : IHookBase
{
    private ILogger _logger;
    private IReloadedHooks _hooks;
    private IScanManager _scanManager;

    public delegate void OpenFile(FileLoadResult* result, uint a2, StringWrap* fileName);
    public IHook<OpenFile> HOOK_OpenFile { get; private set; }

    public delegate int FileExists(StringWrap* fileName);
    private IHook<FileExists> HOOK_FileExists;

    public delegate void OpenFile2(IntPtr a1, StringWrap* fileName, IntPtr @params, IntPtr a4, IntPtr a5);
    private IHook<OpenFile2> HOOK_OpenFile2;

    public FileLogger(ILogger logger, IScanManager scanManager, IReloadedHooks hooks)
    {
        _logger = logger;
        _scanManager = scanManager;
        _hooks = hooks;
    }

    public void Init(string groupSource)
    {
        _scanManager.AddScan(nameof(OpenFile), groupSource, (result)
            => HOOK_OpenFile = _hooks.CreateHook<OpenFile>(OpenFileImpl, result).Activate());
        _scanManager.AddScan(nameof(FileExists), groupSource, (result)
            => HOOK_FileExists = _hooks.CreateHook<FileExists>(FileExistsImpl, result).Activate());
        _scanManager.AddScan(nameof(OpenFile2), groupSource, (result)
            => HOOK_OpenFile2 = _hooks.CreateHook<OpenFile2>(OpenFile2Impl, result).Activate());

        var thread = new Thread(WriteStuff);
        thread.Start();
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
                {
                    _logger.WriteLine($"open (ok): {str}, size=0x{result->FileSize:X8}");
                    AddNotExist(str);
                }
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
                {
                    _logger.WriteLine($"exists (ok): {str}");
                    AddNotExist(str);
                }
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
                AddNotExist(str);
                _logger.WriteLine($"open2: {str}");
            }
        }
    }

    public string ToAdd { get; set; } = "";
    public string FileName { get; set; }
    public HashSet<string> Existing { get; set; } = new HashSet<string>();
    public void Flush(string dir)
    {
        foreach (var path in ToAdd.Split(new[] { Environment.NewLine },
                     StringSplitOptions.RemoveEmptyEntries))
        {
            Existing.Add(path);
        }

        File.AppendAllText(Path.Combine(dir, "filenames_cbt.txt"), ToAdd);
        ToAdd = "";
    }

    public void AddNotExist(string val)
    {
        if (val.StartsWith("data\\"))
            val = val.Substring(5);

        val = val.Replace('\\', '/');

        if (Existing.Contains(val) || ToAdd.Contains(val))
            return;

        ToAdd += val + "\n";
    }

    public void Add(string val)
    {
        ToAdd += val + "\n";
    }

    private void WriteStuff()
    {
        DirectoryInfo dir = Directory.GetParent(Assembly.GetExecutingAssembly().Location);
        while (true)
        {
            Flush(dir.FullName);
            Thread.Sleep(TimeSpan.FromSeconds(1));
        }
    }
}
