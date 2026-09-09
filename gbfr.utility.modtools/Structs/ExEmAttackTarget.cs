using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using gbfr.utility.modtools.Hooks.Behavior;
using gbfr.utility.modtools.Native;

namespace gbfr.utility.modtools.Structs;

public sealed unsafe class ExEmAttackTargetView<T> : IExEmAttackTarget
    where T : unmanaged, IExEmAttackTarget
{
    private readonly T* _ptr;

    public ExEmAttackTargetView(T* ptr) => _ptr = ptr;
    public ExEmAttackTargetView(nint ptr) => _ptr = (T*)ptr;

    public nint NativePointer => (nint)_ptr;

    public EntityRef Target { get => _ptr->Target; set => _ptr->Target = value; }
    public int NumTargetUpdates { get => _ptr->NumTargetUpdates; set => _ptr->NumTargetUpdates = value; }
    public StdVector<AttackTargetPlayerEntry> AttackTargetPlayerList { get => _ptr->AttackTargetPlayerList; set => _ptr->AttackTargetPlayerList = value; }
    public StdUnorderedMapHash64 HashToAttackHateParamMap { get => _ptr->HashToAttackHateParamMap; set => _ptr->HashToAttackHateParamMap = value; }
}

public unsafe struct ExEmAttackTarget : IExEmAttackTarget
{
    public nint NativePointer { get { fixed (ExEmAttackTarget* p = &this) return (nint)p; } }

    public nint __vftable;
    public nint qword8;
    public StdVector<AttackTargetPlayerEntry> AttackTargetPlayerList_; // 0x10
    public StdUnorderedMapHash64 HashToAttackHateParamMap_; // 0x28
    public EntityRef Target_; // 0x78
    public nint field_0x80;
    public nint field_0x88;
    public nint qword90;
    public int qword98;
    public int NumTargetUpdates_;
    public float Score;
    public byte field_A4;
    public byte field_A5;
    public byte field_A6;
    public byte field_A7;
    public int field_A8;
    public int field_AC;
    public nint field_B0;

    public EntityRef Target { get => Target_; set => Target_ = value; }
    public int NumTargetUpdates { get => NumTargetUpdates_; set => NumTargetUpdates_ = value; }
    public StdVector<AttackTargetPlayerEntry> AttackTargetPlayerList { get => AttackTargetPlayerList_; set => AttackTargetPlayerList_ = value; }
    public StdUnorderedMapHash64 HashToAttackHateParamMap { get => HashToAttackHateParamMap_; set => HashToAttackHateParamMap_ = value; }
}

public unsafe struct ExEmAttackTarget_ER : IExEmAttackTarget // 0x170
{
    public nint NativePointer { get { fixed (ExEmAttackTarget_ER* p = &this) return (nint)p; } }

    public nint __vftable;
    public nint qword0x08;
    public nint qword0x10;
    public nint qword0x18;
    public nint qword0x20;
    public nint qword0x28;
    public nint qword0x30;
    public nint qword0x38;
    public nint qword0x40;
    public nint qword0x48;
    public nint qword0x50;
    public nint qword0x58;
    public nint qword0x60;
    public nint qword0x68;
    public nint qword0x70;
    public nint qword0x78;
    public nint qword0x80;
    public nint qword0x88;
    public nint qword0x90;
    public nint qword0x98;
    public nint qword0xA0;
    public nint qword0xA8;
    public nint qword0xB0;
    public nint qword0xB8;
    public StdVector<AttackTargetPlayerEntry> AttackTargetPlayerList_; // 0xC0
    public StdUnorderedMapHash64 HashToAttackHateParamMap_; // 0xD8
    public EntityRef Target_; // 0x98
    public nint field_0xB0;
    public nint field_0xB8;
    public nint qwordC0;
    public int qwordC8;
    public int NumTargetUpdates_;
    public float Score;
    public byte field_A4;
    public byte field_A5;
    public byte field_A6;
    public byte field_A7;
    public int field_A8;
    public int field_AC;
    public nint field_B0;

    public EntityRef Target { get => Target_; set => Target_ = value; }
    public int NumTargetUpdates { get => NumTargetUpdates_; set => NumTargetUpdates_ = value; }
    public StdVector<AttackTargetPlayerEntry> AttackTargetPlayerList { get => AttackTargetPlayerList_; set => AttackTargetPlayerList_ = value; }
    public StdUnorderedMapHash64 HashToAttackHateParamMap { get => HashToAttackHateParamMap_; set => HashToAttackHateParamMap_ = value; }
}

public interface IExEmAttackTarget : IPtr
{
    public EntityRef Target { get; set; }
    public int NumTargetUpdates { get; set; }
    public StdVector<AttackTargetPlayerEntry> AttackTargetPlayerList { get; set; }
    public StdUnorderedMapHash64 HashToAttackHateParamMap { get; set; }
}

public interface IPtr
{
    public nint NativePointer { get; }
}
