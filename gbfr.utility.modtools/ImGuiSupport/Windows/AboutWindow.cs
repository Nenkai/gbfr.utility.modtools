using NenTools.ImGui.Implementation;
using NenTools.ImGui.Interfaces;
using NenTools.ImGui.Interfaces.Shell;

using Reloaded.Mod.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gbfr.utility.modtools.ImGuiSupport.Windows;

public unsafe class AboutWindow : IImGuiComponent
{
    private readonly IImGui _imGui;

    private readonly IModConfig _modConfig;

    public bool IsOverlay => false;
    public bool IsOpen = false;

    public AboutWindow(IImGui imGui, IModConfig modConfig)
    {
        _imGui = imGui;
        _modConfig = modConfig;
    }

    public void RenderMenu(IImGuiShell imGuiShell)
    {
        if (_imGui.MenuItemEx("About Window", "", false, true))
        {
            IsOpen = true;
        }
    }

    public void Render(IImGuiShell imGuiShell)
    {
        if (!IsOpen)
            return;

        if (_imGui.Begin("Log Window"u8, ref IsOpen, 0))
        {
            _imGui.Text($"{_modConfig.ModId} {_modConfig.ModVersion}");
            _imGui.Text($"Made by {_modConfig.ModAuthor}");
            _imGui.Spacing();

            _imGui.Text("Keys:"u8);
            _imGui.Text("- INSERT: Show ImGui Menu"u8);
            _imGui.Spacing();

            _imGui.Text("NOTE: Logs are also saved as a file in the game's directory as 'modtools_log.txt'."u8);
        }

        _imGui.End();
    }
}
