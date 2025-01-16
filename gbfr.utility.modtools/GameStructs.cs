using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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

// Good resource: https://bbs.kanxue.com/thread-270547.htm

// https://github.com/microsoft/STL/blob/881bcadeca4ae9240a132588d9ac983e7b24dbe0/stl/inc/list#L286
public unsafe struct StdListNode // _List_node
{
    public StdListNode* Next;
    public StdListNode* Previous;
    public uint Key;
    public uint Padding;
    public void* Data; // Starting from here is data. Type is templated, it could be anything else inline to this struct i.e a std::vector
}

// https://github.com/microsoft/STL/blob/881bcadeca4ae9240a132588d9ac983e7b24dbe0/stl/inc/list#L755
public unsafe struct StdList // std::list
{
    public StdListNode* Node;
    public uint Size;
}

// https://github.com/microsoft/STL/blob/881bcadeca4ae9240a132588d9ac983e7b24dbe0/stl/inc/xhash#L1960
public unsafe struct StdUnorderedMap
{
    public ulong LoadFactor;
    public StdList List;  
    public StdVector Vec;
    public ulong Mask;
    public ulong MaskIdx;

    public readonly uint Size() => List.Size;
    public StdListNode* Begin() => List.Node->Next;
};

public unsafe struct StdVector
{
    public void* Myfirst;
    public void* Mylast;
    public void* Myend;
}

public unsafe struct sEstHeader
{
    public fixed char name[4];
    public uint NumEntries;
    public uint EntryArrayMapOffset;
    public uint OffsetOfFunctions;
    public uint EntryDataOffsetStart;
    public uint FunctionSize;
    public uint NumFunctionsPerTable;
    public uint _pad_;
}