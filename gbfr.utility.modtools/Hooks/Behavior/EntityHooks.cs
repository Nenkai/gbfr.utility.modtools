using gbfr.utility.modtools.Native;

using NenTools.Reloaded.ScanManager.Interfaces;

using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces;
using Reloaded.Hooks.Definitions.Enums;
using Reloaded.Memory.Interfaces;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.CodeDom;

namespace gbfr.utility.modtools.Hooks.Behavior;

public unsafe class EntityHooks : IHookBase
{
    private readonly ILogger _logger;
    private readonly IReloadedHooks _hooks;
    private readonly IScanManager _scanManager;

    public StdVector<EntityRef>* LoadedEntitiesPtr;
    public nint EnemyStartIndexPtr;

    public delegate void* EntityRef_GetEmAttackTargetExtension(EntityRef* entityRef);
    public EntityRef_GetEmAttackTargetExtension WRAPPER_EntityRef_GetEmAttackTargetExtension;

    public delegate float GetHostilityForPlayer(uint playerIndex);
    public GetHostilityForPlayer WRAPPER_GetHostilityForPlayer;

    public EntityHooks(ILogger logger, IScanManager scanManager, IReloadedHooks hooks)
    {
        _logger = logger;
        _scanManager = scanManager;
        _hooks = hooks;
    }

    public void Init(string groupSource)
    {
        _scanManager.AddScan(nameof(EntityRef_GetEmAttackTargetExtension), groupSource, result
            => WRAPPER_EntityRef_GetEmAttackTargetExtension = _hooks.CreateWrapper<EntityRef_GetEmAttackTargetExtension>(result, out _));

        // TODO: Move to battle hooks.
        _scanManager.AddScan(nameof(GetHostilityForPlayer), groupSource, result
            => WRAPPER_GetHostilityForPlayer = _hooks.CreateWrapper<GetHostilityForPlayer>(result, out _));

        _scanManager.AddScan("LoadedEntitiesPtrAccess", groupSource, addr => LoadedEntitiesPtr = (StdVector<EntityRef>*)(addr + *(int*)(addr + 3) + 7));
        _scanManager.AddScan("EnemyStartIndexAccess", groupSource, addr => EnemyStartIndexPtr = addr + *(int*)(addr + 3) + 7);
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
    public nint ObjReadWithAppend;
    public nint field_58;
    public nint field_60;
    public nint field_68;
    public cObj* EntityObjPtr;
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
public unsafe struct StdUnorderedMapHash64 // size: 0x40
{
    public ulong LoadFactor;
    public StdListHash64 List;
    public StdVector Vec;
    public ulong Mask;
    public ulong MaskIdx;

    public readonly uint Size() => List.Size;
    public StdListNodeHash64* Begin() => List.Node->Next;
};