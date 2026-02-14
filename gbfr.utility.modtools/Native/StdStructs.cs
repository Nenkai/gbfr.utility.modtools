using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace gbfr.utility.modtools.Native;

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

[StructLayout(LayoutKind.Sequential)]
public unsafe struct StdVector<T> where T : unmanaged
{
    public T* Myfirst;
    public T* Mylast;
    public T* Myend;

    public readonly int Size => (int)(Mylast - Myfirst);

    public readonly int Capacity => (int)(Myend - Myfirst);

    public Span<T> AsSpan()
    {
        return new Span<T>(Myfirst, Size);
    }

    public ref T this[int index]
    {
        get
        {
            if ((uint)index >= Size)
                throw new IndexOutOfRangeException();
            return ref Myfirst[index];
        }
    }
}
