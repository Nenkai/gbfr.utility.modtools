using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

using Reloaded.Mod.Interfaces;

using SharedScans.Interfaces;


namespace gbfr.utility.modtools.Hooks.Effects;

public unsafe class EventHooks
{
    private readonly ISharedScans _scans;

    public unsafe delegate nint Event_Unk(EventManager* this_);
    public static HookContainer<Event_Unk> HOOK_EventUnk { get; private set; }

    public EventManager* EventManagerPtr;

    public Dictionary<string, string> Patterns = new()
    {
        [nameof(Event_Unk)] = "55 41 57 41 56 56 57 53 48 83 EC ?? 48 8D 6C 24 ?? 48 C7 45 ?? ?? ?? ?? ?? 48 89 CE C7 01",
    };

    public EventHooks(ISharedScans scans)
    {
        _scans = scans;
    }

    public void Init()
    {
        foreach (var pattern in Patterns)
            _scans.AddScan(pattern.Key, pattern.Value);

        // Maps the bxm file, open the est buffers
        HOOK_EventUnk = _scans.CreateHook<Event_Unk>(HOOK_EventUnkImpl, "a");
    }

    public nint HOOK_EventUnkImpl(EventManager* this_)
    {
        EventManagerPtr = this_;
        return HOOK_EventUnk.Hook.OriginalFunction(this_);
    }
}

public unsafe struct EventManager
{
    public EventTypeEntry Events;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Size = 0xD0)]
public struct EventTypeEntry
{
    public byte Flags;
    public byte field_0x01;
    public byte field_0x02;
    public byte field_0x03;
    public EventType Type;
    public int Id;
}

public enum EventType : int
{
    // system/event/...
    None = 0,
    ct = 1,
    ci = 2,
    cw = 3,
    ev = 4,
    cn = 5,
}