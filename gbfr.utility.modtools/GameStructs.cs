using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace gbfr.utility.modtools;

public unsafe struct FileOpenResult
{
    public void* pFileStorage; // flatark::impl::Flatark::ChunkFileStorage
    public byte* pFileData;
    public ulong FileSize;
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
    public void* Data;
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
};

public unsafe struct StdVector
{
    public void* Myfirst;
    public void* Mylast;
    public void* Myend;
}

public unsafe struct CharacterManager
{
    public fixed byte char0[56];
    public StdUnorderedMap Chara;
    public StdUnorderedMap CharaCostume;
    public StdUnorderedMap CharaExp;
    public StdUnorderedMap CharaExpType;
    public StdUnorderedMap CharaStatus;
    public StdUnorderedMap CharaGem;
    public StdUnorderedMap CharaColor;
    public StdUnorderedMap CharaDrain;
    public StdVector CharaDiff;
    public StdUnorderedMap CharaPowerAdjust;
    public StdUnorderedMap CharaPowerAttenuate;
    public StdVector CharaLevelSync;
    public StdUnorderedMap CharaStatusFate;
    public StdVector CharaInvite;
    public StdUnorderedMap CharaGuestNpcParameter;
    public StdUnorderedMap FormationSlot;
}