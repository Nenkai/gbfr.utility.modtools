using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using gbfr.utility.modtools;
using gbfr.utility.modtools.Native;

namespace gbfr.utility.modtools.Hooks.Effects;

public unsafe struct EffectData
{
    public nint VTable;
    public EffectDataImpl Impl;
}

// Size = 0x1A0
[StructLayout(LayoutKind.Explicit, CharSet = CharSet.Ansi)]
public unsafe struct EffectDataImpl
{
    [FieldOffset(0x00)]
    public nint VTable;

    [FieldOffset(0x08)]
    public int Id;

    [FieldOffset(0x0C)]
    public int Type;

    [FieldOffset(0x18)]
    public fixed byte Name[32];

    [FieldOffset(0x38)]
    public nint NameLength;

    [FieldOffset(0x40)]
    public bool IsLoaded;

    [FieldOffset(0x48)]
    public FileLoadResult ChunkFileStorage;

    [FieldOffset(0x60)]
    public FlatArkCallbackWrapper* ReadCallbackWrapper;

    [FieldOffset(0x68)]
    public StdUnorderedMap EffectMap;
}

public unsafe struct EffectListUnorderedMap
{
    public long Size;
    public nint Unk;
    public EffectListSub* ListAgain;
}

public unsafe struct EffectListSub
{
    public StdListNode* Start; // struct std::pair<int const, struct asset::EffectDataImpl::Est>?
    public StdListNode* End;
}

// Size: 0x40
public unsafe struct Est // asset::EffectDataImpl::Est
{
    public FileLoadResult FileResult;
    public nint pad_0x30;
    public nint pad_0x38;
}
