using gbfr.utility.modtools.Native;

using Reloaded.Hooks.Definitions;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;

using RyoTune.Reloaded;

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace gbfr.utility.modtools.Hooks;

public unsafe class EntityHooks : IHookBase
{
    public StdVector<EntityRef>* LoadedEntitiesPtr;
    public nint EnemyStartIndexPtr;

    public delegate ExEmAttackTarget* EntityRef_GetEmAttackTargetExtension(EntityRef* entityRef);
    public EntityRef_GetEmAttackTargetExtension WRAPPER_EntityRef_GetEmAttackTargetExtension;

    public delegate float GetHostilityForPlayer(uint playerIndex);
    public GetHostilityForPlayer WRAPPER_GetHostilityForPlayer;

    public EntityHooks()
    {

    }

    public void Init()
    {
        Project.Scans.AddScanHook(nameof(EntityRef_GetEmAttackTargetExtension), (result, hooks)
            => WRAPPER_EntityRef_GetEmAttackTargetExtension = hooks.CreateWrapper<EntityRef_GetEmAttackTargetExtension>(result, out _));

        // TODO: Move to battle hooks.
        Project.Scans.AddScanHook(nameof(GetHostilityForPlayer), (result, hooks)
            => WRAPPER_GetHostilityForPlayer = hooks.CreateWrapper<GetHostilityForPlayer>(result, out _));

        Project.Scans.AddScan("LoadedEntitiesPtrAccess", addr => LoadedEntitiesPtr = (StdVector<EntityRef>*)(addr + *(int*)(addr + 3) + 7));
        Project.Scans.AddScan("EnemyStartIndexAccess", addr => EnemyStartIndexPtr = addr + *(int*)(addr + 3) + 7);
    }
}

public unsafe struct LoadedEntityList
{
    public EntityRef* begin;
    public EntityRef* end;
    public EntityRef* cap;
};

public unsafe struct EntityRef
{
    public uint ActorId;
    public EntityWrapper* EntityRefPtr;
    public ulong Rdtsc;
};

public unsafe struct EntityWrapper
{
    public nint field_0;
    public fixed byte Name[0x20];
    public nint NameLength;
    public nint Flags;
    public nint field_38;
    public nint field_40;
    public nint field_48;
    public nint field_50;
    public nint field_58;
    public nint field_60;
    public nint field_68;
    public cObj* EntityObjPtr;
}

public unsafe struct ExEmAttackTarget
{
    public nint __vftable;
    public nint qword8;
    public StdVector<AttackTargetPlayerEntry> AttackTargetPlayerList; // 0x10
    public StdUnorderedMapHash64 HashToAttackHateParamMap; // 0x28
    public EntityRef Target; // 0x78
    public nint field_0x80;
    public nint field_0x88;
    public nint qword90;
    public int qword98;
    public int NumTargetUpdates;
    public float Score;
    public byte field_A4;
    public byte field_A5;
    public byte field_A6;
    public byte field_A7;
    public int field_A8;
    public int field_AC;
    public nint field_B0;
}

public unsafe struct AttackTargetPlayerEntry
{
    
    public nint qword0;
    public nint gap8;
    public nint field_10;
    public AttackTargetPlayer* AttackTarget;
};

public unsafe struct AttackTargetPlayer
{
    public nint __vftable;
    public EntityRef ThisEnemy;
    public EntityRef TargettingPlayer;
    public StdUnorderedMapHash64 HateParams;
    public float WeightMultiplier;
    public int LastTargettedIndex;
};



public unsafe struct cObj
{
    public cObj_vtable* __vftable /*VFT*/;
    public int field_8;
    public fixed byte gapC[12];
    public nint qword18;
    public int dword20;
    public int field_24;
    public nint field_28;
    public nint field_30;
    public nint field_38;
    public int field_40;
    public int field_44;
    public int field_48;
    public int field_4C;
    public nint field_50;
    public nint field_58;
    public nint field_60;
    public nint field_68;
    public nint field_70;
    public nint field_78;
    public nint field_80;
    public nint field_88;
    public nint field_90;
    public nint field_98;
    public nint field_A0;
    public int field_A8;
    public int field_AC;
    public byte field_B0;
    public nint field_B8;
    public nint Extensions;
    public int field_C8;
    public int field_CC;
    public nint field_D0;
    public nint field_D8;
    public nint field_E0;
    public nint field_E8;
    public byte field_F0;
    public byte field_F1;
    public byte field_F2;
    public nint field_F8;
    public nint field_100;
    public int field_108;
    public int field_10C;
    public nint field_110;
    public nint field_118;
    public nint field_120;
    public nint field_128;
    public nint field_130;

    public string GetName()
    {
        fixed (cObj* thisPtr = &this) 
        {
            nint outName = 0;
            __vftable->GetName(thisPtr, (nint)(&outName));

            return Marshal.PtrToStringAnsi(outName);
        }
    }
};

public unsafe struct cObj_vtable
{
    public nint Func0;
    public nint Func1;
    public nint Func2;
    public nint Func3;
    public nint Func4;
    public nint Func5;
    public nint Func6;
    public nint Func7;
    public nint Func8;
    public delegate* unmanaged[Cdecl]<cObj*, nint, nint> GetName;
}



// Stl map 64 bit key

public unsafe struct StdListNodeHash64 // _List_node
{
    public StdListNodeHash64* Next;
    public StdListNodeHash64* Previous;
    public ulong Key;
    public void* Data; // Starting from here is data. Type is templated, it could be anything else inline to this struct i.e a std::vector
}

// https://github.com/microsoft/STL/blob/881bcadeca4ae9240a132588d9ac983e7b24dbe0/stl/inc/list#L755
public unsafe struct StdListHash64 // std::list
{
    public StdListNodeHash64* Node;
    public uint Size;
}

// https://github.com/microsoft/STL/blob/881bcadeca4ae9240a132588d9ac983e7b24dbe0/stl/inc/xhash#L1960
public unsafe struct StdUnorderedMapHash64
{
    public ulong LoadFactor;
    public StdListHash64 List;
    public StdVector Vec;
    public ulong Mask;
    public ulong MaskIdx;

    public readonly uint Size() => List.Size;
    public StdListNodeHash64* Begin() => List.Node->Next;
};