using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

using Reloaded.Mod.Interfaces;

using SharedScans.Interfaces;

using System.Collections.Concurrent.Extended;

namespace gbfr.utility.modtools.Hooks.Effects;

public unsafe class EffectDataHooks
{
    private readonly ISharedScans _scans;

    public unsafe delegate short asset__EffectDataImpl__OpenXmlFile(EffectDataImpl* this_);
    public static WrapperContainer<asset__EffectDataImpl__OpenXmlFile> WRAPPER_asset__EffectDataImpl__Load { get; private set; }

    public unsafe delegate void asset__EffectData__ReadXmlAndOpenEsts(void* this_, void* a2);
    public static HookContainer<asset__EffectData__ReadXmlAndOpenEsts> HOOK_asset__EffectData__ReadXmlAndMapEsts { get; private set; }

    public unsafe delegate bool asset__EffectData__Destructor(EffectData* this_);
    public static HookContainer<asset__EffectData__Destructor> HOOK_asset__EffectData__Destructor { get; private set; }

    public ConcurrentSortedDictionary<string, EffectSet> EffectSets { get; } = [];

    public Dictionary<string, string> Patterns = new()
    {
        [nameof(asset__EffectDataImpl__OpenXmlFile)] = "55 41 57 41 56 41 55 41 54 56 57 53 48 81 EC ?? ?? ?? ?? 48 8D AC 24 ?? ?? ?? ?? 48 83 E4 ?? 48 89 E3 48 89 AB ?? ?? ?? ?? 48 C7 85 ?? ?? ?? ?? ?? ?? ?? ?? 49 89 CC 8B 41",
        [nameof(asset__EffectData__ReadXmlAndOpenEsts)] = "55 41 57 41 56 41 55 41 54 56 57 53 48 81 EC ?? ?? ?? ?? 48 8D AC 24 ?? ?? ?? ?? C5 F8 29 BD ?? ?? ?? ?? C5 F8 29 B5 ?? ?? ?? ?? 48 83 E4 ?? 48 89 E3 48 89 AB ?? ?? ?? ?? 48 C7 85 ?? ?? ?? ?? ?? ?? ?? ?? 83 3A",
        [nameof(asset__EffectData__Destructor)] = "41 57 41 56 41 55 41 54 56 57 53 48 83 EC ?? F6 41"
    };

    public EffectDataHooks(ISharedScans scans)
    {
        _scans = scans;
    }

    public void Init()
    {
        foreach (var pattern in Patterns)
            _scans.AddScan(pattern.Key, pattern.Value);

        // We hook this because character object params are created separately.
        WRAPPER_asset__EffectDataImpl__Load = _scans.CreateWrapper<asset__EffectDataImpl__OpenXmlFile>("a");

        // Maps the bxm file, open the est buffers
        HOOK_asset__EffectData__ReadXmlAndMapEsts = _scans.CreateHook<asset__EffectData__ReadXmlAndOpenEsts>(asset__EffectData__ReadXmlAndOpenEstsImpl, "a");

        // To keep track of unloaded effects
        // NOTE: Removed because the dtor always seems to be called. It seems the Ests are probably passed to another structure.
        // NOTE2: Try to find a way to track down files that are unloaded (if any even are).
        // HOOK_asset__EffectData__Destructor = _scans.CreateHook<asset__EffectData__Destructor>(asset__EffectData__DestructorImpl, "a");

    }

    public bool asset__EffectData__DestructorImpl(EffectData* this_)
    {
        string name = Marshal.PtrToStringAnsi((nint)this_->Impl.Name);
        if (EffectSets.TryGetValue(name, out EffectSet effSet))
        {
            EffectSets.TryRemove(name);
            Console.WriteLine($"Unloaded EffectData for '{name}'");
        }

        return HOOK_asset__EffectData__Destructor.Hook.OriginalFunction(this_);
    }

    public void asset__EffectData__ReadXmlAndOpenEstsImpl(void* this_, void* a2)
    {
        HOOK_asset__EffectData__ReadXmlAndMapEsts.Hook.OriginalFunction(this_, a2);

        EffectData* effectData = (EffectData*)*(ulong*)this_;
        EffectDataImpl* effectImpl = &effectData->Impl;

        string name = Marshal.PtrToStringAnsi((nint)effectImpl->Name);

        var effectSet = new EffectSet()
        {
            ObjId = name,
        };

        if (EffectSets.ContainsKey(name))
            EffectSets[name] = effectSet;
        else
            EffectSets.TryAdd(name, effectSet);

        StdListNode* currentEntry = effectImpl->EffectMap.Begin();
        for (int i = 0; i < effectImpl->EffectMap.Size(); i++)
        {
            Est* est = (Est*)&currentEntry->Data;

            effectSet.EffectIds.Add(currentEntry->Key, new EstFile()
            {
                Id = currentEntry->Key,
                FilePointer = (nint)est->FileResult.FileBuffer,
                FileSize = (nint)est->FileResult.FileSize,
            });

            currentEntry = currentEntry->Next;
        }

        Console.WriteLine($"Loaded '{name}' with {effectSet.EffectIds.Count} effects");
    }
}

public class EffectSet
{
    public string ObjId { get; set; }
    public SortedDictionary<uint, EstFile> EffectIds { get; set; } = [];
}

public class EstFile
{
    public uint Id { get; set; }
    public nint FilePointer { get; set; }
    public nint FileSize { get; set; }
}
