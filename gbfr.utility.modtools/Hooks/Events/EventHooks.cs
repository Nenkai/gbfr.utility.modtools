using NenTools.Reloaded.ScanManager.Interfaces;

using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace gbfr.utility.modtools.Hooks.Events;

public unsafe class EventHooks : IHookBase
{
    private readonly ILogger _logger;
    private readonly IReloadedHooks _hooks;
    private readonly IScanManager _scanManager;

    public unsafe delegate nint Event_Unk(EventManager* this_);
    public static IHook<Event_Unk> HOOK_EventUnk { get; private set; }

    public EventManager* EventManagerPtr;

    public EventHooks(ILogger logger, IScanManager scanManager, IReloadedHooks hooks)
    {
        _logger = logger;
        _scanManager = scanManager;
        _hooks = hooks;
    }

    public void Init(string groupSource)
    {
        _scanManager.AddScan(nameof(Event_Unk), groupSource, result
            => HOOK_EventUnk = _hooks.CreateHook<Event_Unk>(HOOK_EventUnkImpl, result).Activate());
    }

    public nint HOOK_EventUnkImpl(EventManager* this_)
    {
        EventManagerPtr = this_;
        return HOOK_EventUnk.OriginalFunction(this_);
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