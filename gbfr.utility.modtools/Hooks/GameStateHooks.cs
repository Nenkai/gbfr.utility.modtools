using Reloaded.Hooks.Definitions;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using GBFRDataTools.Database;
using GBFRDataTools.Database.Entities;

namespace gbfr.utility.modtools.Hooks;

public unsafe class GameStateHook
{
    private IReloadedHooks _hooks;

    public nint PlayerPosPtr;
    public nint CamPosPtr;
    public nint QuestIdPtr;
    public nint PhaseIdPtr;

    public GameStateHook(IReloadedHooks hooks)
    {
        _hooks = hooks;

    }

    public void Init(IStartupScanner startupScanner)
    {
        // Character Pos
        // note: this is an array of 4 vec4 for each party pos. this is set after an update iteration and likely used as quick lookup table for.. other unknown stuff
        // it is not used for world position computations, these were already done
        // the actual player pos for each BehaviorPlayerBase is in a ModelImpl structure for each BehaviorPlayerBase

        // Find: lea     rax, g_PlayerPosMaybe ([rel $0618E9B0]) - a global to cam stuff
        // (there are multiple cam pos globals though, not sure which one is actually the real value)
        startupScanner.AddMainModuleScan("48 8D 05 ?? ?? ?? ?? C4 C1 78 28 04 04", e =>
        {
            if (!e.Found)
                return;

            nint addr = Process.GetCurrentProcess().MainModule.BaseAddress + e.Offset;
            int instRelOffset = *(int*)(addr + 3) + 7; // Get offset target of instruction - relative, so +7 because size of instruction
            PlayerPosPtr = addr + instRelOffset;
        });

        // Camera Pos
        // Find: lea     rcx, xmmword_7FF62A302120 ([rel $0618E9B0]) - a global to cam stuff
        // (there are multiple cam pos globals though, not sure which one is actually the real value)
        startupScanner.AddMainModuleScan("48 8D 0D ?? ?? ?? ?? C5 78 28 2C 08", e =>
        {
            if (!e.Found)
                return;

            nint addr = Process.GetCurrentProcess().MainModule.BaseAddress + e.Offset;
            int instRelOffset = *(int*)(addr + 3) + 7; // Get offset target of instruction - relative, so +7 because size of instruction
            CamPosPtr = addr + instRelOffset;
        });

        // Quest id (bgm related code?)
        // Find (cmp     edi, cs:g_QuestId)
        startupScanner.AddMainModuleScan("3B 3D ?? ?? ?? ?? 0F 85 ?? ?? ?? ?? 4C 8D 6C 24", e =>
        {
            if (!e.Found)
                return;

            nint addr = Process.GetCurrentProcess().MainModule.BaseAddress + e.Offset;
            QuestIdPtr = addr + *(int*)(addr + 2) + 6; // +7 because size of instruction
        });

        startupScanner.AddMainModuleScan("C7 05 ?? ?? ?? ?? ?? ?? ?? ?? C5 F8 57 C0 C5 F8 11 05 ?? ?? ?? ?? C5 F8 11 05 ?? ?? ?? ?? C7 05 ?? ?? ?? ?? ?? ?? ?? ?? C5 F0 57 C9", e =>
        {
            nint addr = Process.GetCurrentProcess().MainModule.BaseAddress + e.Offset;
            PhaseIdPtr = addr + *(int*)(addr + 2) + 10; // +10 because size of instruction
        });

        
    }
}
