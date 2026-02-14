using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gbfr.utility.modtools;

public unsafe struct FileLoadResult
{
    public FlatArkChunkFileStorage* ChunkFileStorage; // flatark::impl::Flatark::ChunkFileStorage
    public void* FileBuffer;
    public ulong FileSize;
}

public unsafe struct FlatArkChunkFileStorage // flatark::impl::Flatark::ChunkFileStorage
{
    public nint VTable;
    public void* FileBuffer;
}

// Size: 0x50
public unsafe struct FlatArkCallbackWrapper
{
    public nint VTable;
    public uint field_0x04;
    public uint field_0x08;
    public void* Callback;
    public void* Arg;
}

public unsafe struct StringWrap
{
    public char* pStr;
    public char* StringSize;
}

public unsafe struct sEstHeader
{
    public fixed byte name[4];
    public uint NumEntries;
    public uint EntryArrayMapOffset;
    public uint OffsetOfFunctions;
    public uint EntryDataOffsetStart;
    public uint FunctionSize;
    public uint NumFunctionsPerTable;
    public uint _pad_;
}

public unsafe struct EstFunction
{
    public uint UnkFlag;
    public uint FuncName;
    public uint Size;
    public uint Offset;
}

public unsafe struct EstSinAnm
{
    float start;
    float r_start;
    float cycle;
    float range;
    float cycle_add;
    float range_add;
    float range_mul;
};

public unsafe struct EstSizeSinAnm
{
    uint flag;
    EstSinAnm width;
    EstSinAnm height;
};

[StructLayout(LayoutKind.Explicit)]
public unsafe struct BehaviorTreeComponent
{
    [FieldOffset(0x08)]
    public uint Guid;

    [FieldOffset(0x0C)]
    public uint ParentGuid;
}

[StructLayout(LayoutKind.Explicit)]
public unsafe struct ActionComponent
{
    [FieldOffset(0x00)]
    public BehaviorTreeComponent BehaviorTreeComponent;
}

[StructLayout(LayoutKind.Explicit)]
public unsafe struct DebugPrintAction
{
    [FieldOffset(0x00)]
    public ActionComponent ActionComponent;

    // Reflection name says: 'sys::String'
    // Reflection type set to: 'lib::HashedString'
    // Actually points to: lib::HashedStringMap<uint,char>::Node
    // ^ Used by sys::String ctor anyway
    [FieldOffset(0x30)]
    public HashedStringMapNode* saveString_;

    [FieldOffset(0x38)]
    public uint logType_;

    [FieldOffset(0x3C)]
    public uint outputTiming_;

    [FieldOffset(0x40)]
    public uint outputPlace_;

    [FieldOffset(0x44)]
    public Vector2 outputScreenPosition_;

    [FieldOffset(0x4c)]
    public int field_0x4c;
}

// Size: 0x40
public unsafe struct HashedStringMapNode // lib::HashedStringMap<unsigned int,char>::Node
{
    public void* vtable;
    public void* field_0x08;
    public void* field_0x10;
    public void* field_0x18;
    public void* field_0x20;
    public uint Hash;
    public void* StringPtr;
    public ulong field_0x38;
}