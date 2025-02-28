using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharedScans.Interfaces;

using gbfr.utility.modtools.Hooks.Managers;
using gbfr.utility.modtools.ImGuiSupport;
using System.Runtime.InteropServices;

namespace gbfr.utility.modtools.Hooks.Fsm;

public unsafe class DebugPrintActionHook
{
    private ISharedScans _scans;

    public delegate void DebugPrintAction_Execute(DebugPrintAction* this_);
    private HookContainer<DebugPrintAction_Execute> HOOK_DebugPrintAction_Execute;

    public Dictionary<string, string> Patterns = new()
    {
        [nameof(DebugPrintAction_Execute)] = "83 79 ?? ?? 74 ?? C3 83 79",
    };

    public DebugPrintActionHook(ISharedScans scans)
    {
        _scans = scans;
    }

    public void Init()
    {
        foreach (var pattern in Patterns)
            _scans.AddScan(pattern.Key, pattern.Value);

        HOOK_DebugPrintAction_Execute = _scans.CreateHook<DebugPrintAction_Execute>(DebugPrintAction_ExecuteImpl, "a");
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
            OverlayLogger.Instance.AddMessage($"[FSM] [Node {this_->ActionComponent.BehaviorTreeComponent.ParentGuid}] {msg}");
        }
    }
}
