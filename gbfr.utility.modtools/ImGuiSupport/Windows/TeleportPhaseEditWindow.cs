using DearImguiSharp;

using gbfr.utility.modtools.Hooks;
using gbfr.utility.modtools.Hooks.Effects;

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

public unsafe class TeleportPhaseEditWindow : IImguiWindow, IImguiMenuComponent
{
    public bool IsOverlay => false;
    public bool IsOpen = false;

    private TeleportHooks _teleportHooks;

    public TeleportPhaseEditWindow(TeleportHooks teleportHooks)
    {
        _teleportHooks = teleportHooks;
    }

    public void BeginMenuComponent()
    {
        if (ImGui.MenuItemEx("Teleport Phases", "", "", false, true))
        {
            IsOpen = true;
        }
    }

    public void Render(ImguiSupport imguiSupport)
    {
        if (!IsOpen)
            return;

        if (ImGui.Begin("Teleport Phases", ref IsOpen, 0))
        {
            for (int i = 0; i < TeleportHooks.TableSize; i++)
            {
                ImGui.InputInt($"Phase[{i}]", ref Unsafe.AsRef<int>((int*)_teleportHooks.TeleportPhaseTablePtr + i), 1, 1, 0);
            }

            Vector2 vec = Vector2.Zero;
            if (ImGui.Button("Jump! (Phase[0])", new ImVec2(&vec)))
            {
                uint id = *(uint*)_teleportHooks.TeleportPhaseTablePtr;
                _teleportHooks.WRAPPER_PhaseJump(id, null, 0xFF000000);
            }

            ImGui.End();
        }
    }
}
