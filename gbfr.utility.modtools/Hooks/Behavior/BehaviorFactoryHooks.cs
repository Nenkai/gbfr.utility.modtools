using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;

using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces;

using RyoTune.Reloaded;

namespace gbfr.utility.modtools.Hooks.Behavior;

public unsafe class BehaviorFactoryHooks : IHookBase
{
    private ILogger _logger;

    public unsafe delegate void BehaviorFactory_RegisterClass(void* @this, void* outUnk, uint* objId, ClassInfo* classInfo);
    public static IHook<BehaviorFactory_RegisterClass> HOOK_BehaviorFactory_RegisterClass { get; private set; }

    public BehaviorFactoryHooks(ILogger logger)
    {
        _logger = logger;
    }

    public void Init()
    {
        Project.Scans.AddScanHook(nameof(BehaviorFactory_RegisterClass), (result, hooks)
            => HOOK_BehaviorFactory_RegisterClass = hooks.CreateHook<BehaviorFactory_RegisterClass>(BehaviorFactory_RegisterClassImpl, result).Activate());
    }

    public void BehaviorFactory_RegisterClassImpl(void* @this, void* outUnk, uint* objId, ClassInfo* classInfo)
    {
        string name = Marshal.PtrToStringUTF8((nint)classInfo->Name);

        uint objId_ = *objId;
        _logger.WriteLine($"{(ObjIdType)(objId_ >> 16)}{objId_ & 0xFFFF:X4} ({objId_:X8}) = {name}");

        HOOK_BehaviorFactory_RegisterClass.OriginalFunction(@this, outUnk, objId, classInfo);
    }
}

public enum ObjIdType
{
    Pl = 0x01,
    Em = 0x02,
    Wp = 0x03,
    Et = 0x04,
    Ef = 0x05,
    It = 0x07,
    Sc = 0x09,
    Bg = 0x0C,
    Bh = 0x0E,
    Ba = 0x0F,
    Fp = 0x100,
    Fe = 0x101,
    Fn = 0x102,
    We = 0x103,
    Wn = 0x104,
    Np = 0x10A,
    Tr = 0x10B,
    Bt = 0x10C
}

public unsafe struct ClassInfo /* BehaviorFactory::ClassInfo */
{
    public int ObjId;
    public int field_4;
    public byte* Name;
    public void* CtorCb;
    public void* GetClassUnk;
    public void* GetClassUnk2;
    public int field_28;
}
