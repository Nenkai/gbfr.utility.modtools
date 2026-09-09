using gbfr.utility.modtools.ImGuiSupport;

using NenTools.ImGui.Interfaces.Shell;
using NenTools.Reloaded.ScanManager.Interfaces;

using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace gbfr.utility.modtools.Hooks.Fsm;

public unsafe class DebugPrintActionHook : IHookBase
{
    private readonly ILogger _logger;
    private readonly IReloadedHooks _hooks;
    private readonly IScanManager _scanManager;
    private readonly IImGuiShell _imGuiShell;

    public delegate void DebugPrintAction_Execute(DebugPrintAction* this_);
    private IHook<DebugPrintAction_Execute> HOOK_DebugPrintAction_Execute;

    public DebugPrintActionHook(ILogger logger, IScanManager scanManager, IReloadedHooks hooks,
        IImGuiShell imGuiShell)
    {
        _logger = logger;
        _scanManager = scanManager;
        _hooks = hooks;
        _imGuiShell = imGuiShell;
    }

    public void Init(string groupSource)
    {
        _scanManager.AddScan(nameof(DebugPrintAction_Execute), groupSource, result
            => HOOK_DebugPrintAction_Execute = _hooks.CreateHook<DebugPrintAction_Execute>(DebugPrintAction_ExecuteImpl, result).Activate());
    }

    public void DebugPrintAction_ExecuteImpl(DebugPrintAction* this_)
    {
        // We don't need it. (for some reason it crashes oddly sometimes??) it's just an assignment. 
        // HOOK_DebugPrintAction_Execute.Hook.OriginalFunction(this_);

        { // Original block
            if (this_->outputTiming_ == 0 && this_->field_0x4c == 0)
                this_->field_0x4c = 1;
        }

        if (this_->saveString_ != null && this_->saveString_->StringPtr != null)
        {
            string msg = Marshal.PtrToStringUTF8((nint)this_->saveString_->StringPtr);
            _imGuiShell.LogWriteLine(nameof(DebugPrintActionHook), $"[FSM] [Node {this_->ActionComponent.BehaviorTreeComponent.ParentGuid}] {msg}");
        }
    }
}
