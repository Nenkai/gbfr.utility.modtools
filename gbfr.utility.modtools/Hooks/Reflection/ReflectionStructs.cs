using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace gbfr.utility.modtools.Hooks.Reflection;

[StructLayout(LayoutKind.Explicit, CharSet = CharSet.Ansi)]
public unsafe struct ObjectDef
{
    [FieldOffset(0x00)]
    public byte* pName;

    [FieldOffset(0x08)]
    public uint dwHash;

    [FieldOffset(0x10)]
    public IObjectType* pObjectType;
}

[StructLayout(LayoutKind.Explicit)]
public unsafe struct IObjectType
{
    [FieldOffset(0)]
    public void* pVtable;

    [FieldOffset(0x08)]
    public byte* pName;

    [FieldOffset(0x28)]
    public IAttributeList* pAttrList;

    [FieldOffset(0x50)]
    public byte* pTypeName;
}

[StructLayout(LayoutKind.Explicit)]
public unsafe struct IAttributeList
{
    [FieldOffset(0x00)]
    public IAttribute** pBegin;

    [FieldOffset(0x08)]
    public IAttribute** pEnd;

    [FieldOffset(0x10)]
    public IAttribute** pCapacity;
}

[StructLayout(LayoutKind.Explicit)]
public unsafe struct IAttribute
{
    [FieldOffset(0)]
    public void* pVtable;

    [FieldOffset(0x08)]
    public int field_0x08;

    [FieldOffset(0x10)]
    public byte* pAttrName;

    [FieldOffset(0x18)]
    public byte* pTypeName;

    [FieldOffset(0x20)]
    public int dwOffset;

    [FieldOffset(0x24)]
    public int field_0x24;

    [FieldOffset(0x28)]
    public int field_0x28;

    [FieldOffset(0x2C)]
    public int field_0x2C;

    [FieldOffset(0x30)]
    public int field_0x30;

    [FieldOffset(0x34)]
    public int field_0x34;

    [FieldOffset(0x38)]
    public int field_0x38;
}
