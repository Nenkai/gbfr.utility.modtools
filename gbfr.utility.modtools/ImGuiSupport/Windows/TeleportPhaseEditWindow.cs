using gbfr.utility.modtools.Hooks;
using gbfr.utility.modtools.Hooks.Effects;

using NenTools.ImGui.Interfaces;
using NenTools.ImGui.Interfaces.Shell;

using Reloaded.Mod.Interfaces;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace gbfr.utility.modtools.ImGuiSupport.Windows;

public unsafe class TeleportPhaseEditWindow : IImGuiComponent
{
    private readonly IImGui _imGui;

    public bool IsOverlay => false;
    public bool IsOpen = false;

    private TeleportHooks _teleportHooks;

    public TeleportPhaseEditWindow(IImGui imGui, TeleportHooks teleportHooks)
    {
        _imGui = imGui;
        _teleportHooks = teleportHooks;
    }

    public void RenderMenu(IImGuiShell imGuiShell)
    {
        if (_imGui.MenuItemEx("Teleport Phases", "", false, true))
        {
            IsOpen = true;
        }
    }

    public void Render(IImGuiShell imGuiShell)
    {
        if (!IsOpen)
            return;

        if (_imGui.Begin("Teleport Phases", ref IsOpen, 0))
        {
            for (int i = 0; i < TeleportHooks.TableSize; i++)
            {
                _imGui.InputInt($"Phase[{i}]", ref Unsafe.AsRef<int>((int*)_teleportHooks.TeleportPhaseTablePtr + i));
            }

            if (_imGui.Button("Jump! (Phase[0])"u8))
            {
                uint id = *(uint*)_teleportHooks.TeleportPhaseTablePtr;
                _teleportHooks.WRAPPER_PhaseJump(id, null, 0xFF000000);
            }

            _imGui.End();
        }
    }
}
