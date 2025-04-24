using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using DearImguiSharp;

using gbfr.utility.modtools.Hooks;
using gbfr.utility.modtools.Hooks.Effects;

namespace gbfr.utility.modtools.ImGuiSupport.Windows;

public unsafe class GameOverlay : IImguiWindow
{
    public bool IsOverlay => true;

    private bool _open = true;

    private GameStateHook _gameStateHook;
    private EventHooks _eventHooks;
    public GameOverlay(GameStateHook gameStateHook, EventHooks eventHooks)
    {
        _gameStateHook = gameStateHook;
        _eventHooks = eventHooks;
    }

    public void BeginMenuComponent()
    {
        ImGui.MenuItemBoolPtr("Enable Overlay", "", ref _open, true);
    }

    public void Render(ImguiSupport imguiSupport)
    {
        if (!_open)
            return;

        float barHeight = 0;
        if (imguiSupport.IsMenuOpen)
            barHeight += ImGui.GetFrameHeight();

        var viewport = ImGui.GetMainViewport();
        ImGui.SetNextWindowBgAlpha(0.35f);
        if (ImGui.Begin("overlay", ref _open, (int)(ImGuiWindowFlags.NoDecoration |
            ImGuiWindowFlags.NoDocking |
            ImGuiWindowFlags.AlwaysAutoResize |
            ImGuiWindowFlags.NoSavedSettings |
            ImGuiWindowFlags.NoFocusOnAppearing |
            ImGuiWindowFlags.NoNav)))
        {
            Vector3 playerPos = *(Vector3*)_gameStateHook.PlayerPosPtr;
            ImGui.Text($"Player Pos: <{playerPos.X:F2}, {playerPos.Y:F2}, {playerPos.Z:F2}>");

            Vector3 camPos = *(Vector3*)(_gameStateHook.CamPosPtr + 0x10);
            ImGui.Text($"Camera Pos: <{camPos.X:F2}, {camPos.Y:F2}, {camPos.Z:F2}>");

            ImGui.Spacing();

            ImGui.Text($"Last Quest ID: {*(int*)_gameStateHook.QuestIdPtr:X6}");
            ImGui.Text($"Phase ID: p{*(ushort*)_gameStateHook.PhaseIdPtr:X3}");

            if (_eventHooks.EventManagerPtr != null)
            {
                int cnt = 0;
                for (int i = 0; i < 8; i++)
                {
                    var @event = (&_eventHooks.EventManagerPtr->Events)[i];
                    if (@event.Type == 0)
                        break;

                    cnt++;
                }

                ImGui.Text($"Events ({cnt}/8)");
                for (int i = 0; i < 8; i++)
                {
                    var @event = (&_eventHooks.EventManagerPtr->Events)[i];
                    if (@event.Type == 0)
                        break;

                    ImGui.Text($"- Event[{i}] = {@event.Type}{@event.Id:X4}");
                }
            }

            var vecInternal = new ImVec2.__Internal();
            var vector = new ImVec2(&vecInternal); // Heap allocation
            vector.X = ImGui.GetIO().DisplaySize.X - ImGui.GetWindowWidth() - 10;
            vector.Y = barHeight + 5 /* padding */;

            ImGui.SetWindowPosVec2(vector, 1);
            ImGui.End();
        }
    }
}
