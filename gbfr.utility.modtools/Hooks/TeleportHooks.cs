using Reloaded.Hooks.Definitions;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using RyoTune.Reloaded;
using Reloaded.Memory.Interfaces;

namespace gbfr.utility.modtools.Hooks;

public unsafe class TeleportHooks : IHookBase
{
    public nint TeleportPhaseTablePtr;
    public const int TableSize = 3;

    public delegate void PhaseJump(uint phaseId, void* a2, uint a3);
    public PhaseJump WRAPPER_PhaseJump { get; private set; }

    public TeleportHooks()
    {

    }

    public void Init()
    {
        Project.Scans.AddScan("TeleportPhaseTable", addr =>
        {
            TeleportPhaseTablePtr = addr;
            Reloaded.Memory.Memory.Instance.ChangeProtection((nuint)addr, TableSize * sizeof(int), Reloaded.Memory.Enums.MemoryProtection.ReadWriteExecute);
        });

        Project.Scans.AddScanHook(nameof(PhaseJump), (addr, hooks) =>
        {
            WRAPPER_PhaseJump = hooks.CreateWrapper<PhaseJump>(addr, out _);
        });
    }
}
