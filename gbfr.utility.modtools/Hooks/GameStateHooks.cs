using NenTools.Reloaded.ScanManager.Interfaces;

using Reloaded.Hooks.Definitions;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gbfr.utility.modtools.Hooks;

public unsafe class GameStateHook : IHookBase
{
    public nint PlayerPosPtr;
    public nint CamPosPtr;
    public nint QuestIdPtr;
    public nint PhaseIdPtr;

    private ILogger _logger;
    private IReloadedHooks _hooks;
    private IScanManager _scanManager;

    public GameStateHook(ILogger logger, IScanManager scanManager, IReloadedHooks hooks)
    {
        _logger = logger;
        _scanManager = scanManager;
        _hooks = hooks;
    }

    public void Init(string groupSource)
    {
        // Character Pos
        // note: this is an array of 4 vec4 for each party pos. this is set after an update iteration and likely used as quick lookup table for.. other unknown stuff
        // it is not used for world position computations, these were already done
        // the actual player pos for each BehaviorPlayerBase is in a ModelImpl structure for each BehaviorPlayerBase

        // Find: lea     rax, g_PlayerPosMaybe ([rel $0618E9B0]) - a global to cam stuff
        // (there are multiple cam pos globals though, not sure which one is actually the real value)
        _scanManager.AddScan("CharacterPosAccess", groupSource, addr => PlayerPosPtr = addr + *(int*)(addr + 3) + 7); // Get offset target of instruction - relative, so +7 because size of instruction

        // Camera Pos
        // Find: lea     rcx, xmmword_7FF62A302120 ([rel $0618E9B0]) - a global to cam stuff
        // (there are multiple cam pos globals though, not sure which one is actually the real value)
        _scanManager.AddScan("CamPosAccess", groupSource, addr => CamPosPtr = addr + *(int*)(addr + 3) + 7); // Get offset target of instruction - relative, so +7 because size of instruction

        // Quest id (bgm related code?)
        // Find (cmp     edi, cs:g_QuestId)
        _scanManager.AddScan("QuestIdAccess", groupSource, addr => QuestIdPtr = addr + *(int*)(addr + 2) + 6); // +7 because size of instruction
        _scanManager.AddScan("PhaseIdAccess", groupSource, addr => PhaseIdPtr = addr + *(int*)(addr + 2) + 10); // +10 because size of instruction 
    }
}
