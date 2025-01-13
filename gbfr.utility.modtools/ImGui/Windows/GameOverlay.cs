using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using DearImguiSharp;

using gbfr.utility.modtools.Hooks;

namespace gbfr.utility.modtools.ImGuiSupport.Windows;

public unsafe class GameOverlay : IImguiWindow
{
    public bool IsOverlay => true;

    private bool _open = true;

    private GameStateHook _gameStateHook;
    public GameOverlay(GameStateHook gameStateHook)
    {
        _gameStateHook = gameStateHook;
    }

    public void BeginMenuComponent()
    {
        ImGui.MenuItemBoolPtr("Enable Overlay", "", ref _open, true);
    }

    public void Render()
    {
        if (!_open)
            return;

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

            var vecInternal = new ImVec2.__Internal();
            var vector = new ImVec2(&vecInternal); // Heap allocation
            vector.X = ImGui.GetIO().DisplaySize.X - ImGui.GetWindowWidth() - 10;
            vector.Y = 10;

            ImGui.SetWindowPosVec2(vector, 1);
            ImGui.End();
        }
    }
}
