using Reloaded.Hooks.Definitions;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using RyoTune.Reloaded;

namespace gbfr.utility.modtools.Hooks;

public unsafe class GameStateHook : IHookBase
{
    public nint PlayerPosPtr;
    public nint CamPosPtr;
    public nint QuestIdPtr;
    public nint PhaseIdPtr;

    public GameStateHook()
    {

    }

    public void Init()
    {
        // Character Pos
        // note: this is an array of 4 vec4 for each party pos. this is set after an update iteration and likely used as quick lookup table for.. other unknown stuff
        // it is not used for world position computations, these were already done
        // the actual player pos for each BehaviorPlayerBase is in a ModelImpl structure for each BehaviorPlayerBase

        // Find: lea     rax, g_PlayerPosMaybe ([rel $0618E9B0]) - a global to cam stuff
        // (there are multiple cam pos globals though, not sure which one is actually the real value)
        Project.Scans.AddScan("CharacterPosAccess", addr => PlayerPosPtr = addr + *(int*)(addr + 3) + 7); // Get offset target of instruction - relative, so +7 because size of instruction

        // Camera Pos
        // Find: lea     rcx, xmmword_7FF62A302120 ([rel $0618E9B0]) - a global to cam stuff
        // (there are multiple cam pos globals though, not sure which one is actually the real value)
        Project.Scans.AddScan("CamPosAccess", addr => CamPosPtr = addr + *(int*)(addr + 3) + 7); // Get offset target of instruction - relative, so +7 because size of instruction

        // Quest id (bgm related code?)
        // Find (cmp     edi, cs:g_QuestId)
        Project.Scans.AddScan("QuestIdAccess", addr => QuestIdPtr = addr + *(int*)(addr + 2) + 6); // +7 because size of instruction
        Project.Scans.AddScan("PhaseIdAccess", addr => PhaseIdPtr = addr + *(int*)(addr + 2) + 10); // +10 because size of instruction 
    }
}
