using NenTools.Reloaded.ScanManager.Interfaces;

using Reloaded.Hooks.Definitions;
using Reloaded.Memory.Interfaces;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gbfr.utility.modtools.Hooks;

public unsafe class TeleportHooks : IHookBase
{
    private ILogger _logger;
    private IReloadedHooks _hooks;
    private IScanManager _scanManager;

    public nint TeleportPhaseTablePtr;
    public const int TableSize = 3;

    public delegate void PhaseJump(uint phaseId, void* a2, uint a3);
    public PhaseJump WRAPPER_PhaseJump { get; private set; }

    public TeleportHooks(ILogger logger, IScanManager scanManager, IReloadedHooks hooks)
    {
        _logger = logger;
        _scanManager = scanManager;
        _hooks = hooks;
    }

    public void Init(string groupSource)
    {
        _scanManager.AddScan("TeleportPhaseTable", groupSource, addr =>
        {
            TeleportPhaseTablePtr = addr;
            Reloaded.Memory.Memory.Instance.ChangeProtection((nuint)addr, TableSize * sizeof(int), Reloaded.Memory.Enums.MemoryProtection.ReadWriteExecute);
        });

        _scanManager.AddScan(nameof(PhaseJump), groupSource, addr =>
        {
            WRAPPER_PhaseJump = _hooks.CreateWrapper<PhaseJump>(addr, out _);
        });
    }
}
