using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using RyoTune.Reloaded;

using static gbfr.utility.modtools.Hooks.Managers.CharacterManagerHook;

using gbfr.utility.modtools.Hooks.Managers;
using gbfr.utility.modtools.ImGuiSupport;

using Reloaded.Hooks.Definitions;

namespace gbfr.utility.modtools.Hooks.Fsm;

public unsafe class DebugPrintActionHook : IHookBase
{
    public delegate void DebugPrintAction_Execute(DebugPrintAction* this_);
    private IHook<DebugPrintAction_Execute> HOOK_DebugPrintAction_Execute;

    public DebugPrintActionHook()
    {

    }

    public void Init()
    {
        Project.Scans.AddScanHook(nameof(DebugPrintAction_Execute), (result, hooks)
            => HOOK_DebugPrintAction_Execute = hooks.CreateHook<DebugPrintAction_Execute>(DebugPrintAction_ExecuteImpl, result).Activate());

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
