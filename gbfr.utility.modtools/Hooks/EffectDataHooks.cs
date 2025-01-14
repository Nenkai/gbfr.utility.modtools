using Reloaded.Mod.Interfaces;

using SharedScans.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gbfr.utility.modtools.Hooks;

public unsafe class EffectDataHooks
{
    private readonly ISharedScans _scans;

    public unsafe delegate short asset__EffectDataImpl__Load(void* this_);
    public static HookContainer<asset__EffectDataImpl__Load> HOOK_asset__EffectDataImpl__Load { get; private set; }

    public Dictionary<string, string> Patterns = new()
    {
        [nameof(asset__EffectDataImpl__Load)] = "55 41 57 41 56 41 55 41 54 56 57 53 48 81 EC ?? ?? ?? ?? 48 8D AC 24 ?? ?? ?? ?? 48 83 E4 ?? 48 89 E3 48 89 AB ?? ?? ?? ?? 48 C7 85 ?? ?? ?? ?? ?? ?? ?? ?? 49 89 CC 8B 41",
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
        HOOK_asset__EffectDataImpl__Load = _scans.CreateHook<asset__EffectDataImpl__Load>(asset__EffectDataImpl__LoadImpl, "a");
    }

    public unsafe short asset__EffectDataImpl__LoadImpl(void* this_)
    {
        var res = HOOK_asset__EffectDataImpl__Load.Hook.OriginalFunction(this_);
        return res;
    }
}
