using gbfr.utility.modtools.Hooks;
using gbfr.utility.modtools.Hooks.Events;

using NenTools.ImGui.Implementation;
using NenTools.ImGui.Interfaces;
using NenTools.ImGui.Interfaces.Shell;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace gbfr.utility.modtools.ImGuiSupport.Windows;

public unsafe class GameOverlay : IImGuiComponent
{
    private readonly IImGui _imGui;
    private readonly GameStateHook _gameStateHook;
    private readonly EventHooks _eventHooks;

    public bool IsOverlay => true;

    private bool _open = true;

    private const ImGuiWindowFlags WindowFlags = ImGuiWindowFlags.ImGuiWindowFlags_NoDecoration |
              ImGuiWindowFlags.ImGuiWindowFlags_NoDocking |
              ImGuiWindowFlags.ImGuiWindowFlags_AlwaysAutoResize |
              ImGuiWindowFlags.ImGuiWindowFlags_NoSavedSettings |
              ImGuiWindowFlags.ImGuiWindowFlags_NoFocusOnAppearing |
              ImGuiWindowFlags.ImGuiWindowFlags_NoNav;


    public GameOverlay(IImGui imGui, GameStateHook gameStateHook, EventHooks eventHooks)
    {
        _imGui = imGui;
        _gameStateHook = gameStateHook;
        _eventHooks = eventHooks;
    }

    public void RenderMenu(IImGuiShell imGuiShell)
    {
        _imGui.MenuItemBoolPtr("Enable Overlay"u8, ""u8, ref _open, true);
    }

    public void Render(IImGuiShell imGuiShell)
    {
        if (!_open)
            return;

        float barHeight = 0;
        if (imGuiShell.IsMainMenuOpen)
            barHeight += _imGui.GetFrameHeight();

        var viewport = _imGui.GetMainViewport();
        _imGui.SetNextWindowBgAlpha(0.35f);
        if (_imGui.Begin("overlay"u8, ref _open,
            WindowFlags))
        {
            Vector3 playerPos = *(Vector3*)_gameStateHook.PlayerPosPtr;
            _imGui.Text($"Player Pos: <{playerPos.X:F2}, {playerPos.Y:F2}, {playerPos.Z:F2}>");

            Vector3 camPos = *(Vector3*)(_gameStateHook.CamPosPtr + 0x10);
            _imGui.Text($"Camera Pos: <{camPos.X:F2}, {camPos.Y:F2}, {camPos.Z:F2}>");

            _imGui.Spacing();

            _imGui.Text($"Last Quest ID: {*(int*)_gameStateHook.QuestIdPtr:X6}");
            _imGui.Text($"Phase ID: p{*(ushort*)_gameStateHook.PhaseIdPtr:X3}");

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

                _imGui.Text($"Events ({cnt}/8)");
                for (int i = 0; i < 8; i++)
                {
                    var @event = (&_eventHooks.EventManagerPtr->Events)[i];
                    if (@event.Type == 0)
                        break;

                    _imGui.Text($"- Event[{i}] = {@event.Type}{@event.Id:X4}");
                }
            }

            var vector = new Vector2();
            vector.X = _imGui.GetIO().DisplaySize.X - _imGui.GetWindowWidth() - 10;
            vector.Y = barHeight + 5 /* padding */;

            _imGui.SetWindowPos(vector, ImGuiCond.ImGuiCond_Always);
            _imGui.End();
        }
    }
}
