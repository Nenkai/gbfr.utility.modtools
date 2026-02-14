using System;
using System.Collections.Concurrent;
using System.Collections.Concurrent.Extended;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

using gbfr.utility.modtools.Native;

using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces;

using RyoTune.Reloaded;


using static gbfr.utility.modtools.Hooks.Managers.CharacterManagerHook;

namespace gbfr.utility.modtools.Hooks.Effects;

public unsafe class EffectDataHooks : IHookBase
{
    public unsafe delegate short asset__EffectDataImpl__OpenXmlFile(EffectDataImpl* this_);
    public static asset__EffectDataImpl__OpenXmlFile WRAPPER_asset__EffectDataImpl__Load { get; private set; }

    public unsafe delegate void asset__EffectData__ReadXmlAndOpenEsts(void* this_, void* a2);
    public static IHook<asset__EffectData__ReadXmlAndOpenEsts> HOOK_asset__EffectData__ReadXmlAndMapEsts { get; private set; }

    public unsafe delegate bool asset__EffectData__Destructor(EffectData* this_);
    public static IHook<asset__EffectData__Destructor> HOOK_asset__EffectData__Destructor { get; private set; }

    public ConcurrentSortedDictionary<string, EffectSet> EffectSets { get; } = [];

    public EffectDataHooks()
    {

    }

    public void Init()
    {
        // We hook this because character object params are created separately.
        Project.Scans.AddScanHook(nameof(asset__EffectDataImpl__OpenXmlFile), (result, hooks)
            => WRAPPER_asset__EffectDataImpl__Load = hooks.CreateWrapper<asset__EffectDataImpl__OpenXmlFile>(result, out _));

        // Maps the bxm file, open the est buffers
        Project.Scans.AddScanHook(nameof(asset__EffectData__ReadXmlAndOpenEsts), (result, hooks)
            => HOOK_asset__EffectData__ReadXmlAndMapEsts = hooks.CreateHook<asset__EffectData__ReadXmlAndOpenEsts>(asset__EffectData__ReadXmlAndOpenEstsImpl, result).Activate());

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

        return HOOK_asset__EffectData__Destructor.OriginalFunction(this_);
    }

    public void asset__EffectData__ReadXmlAndOpenEstsImpl(void* this_, void* a2)
    {
        HOOK_asset__EffectData__ReadXmlAndMapEsts.OriginalFunction(this_, a2);

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
