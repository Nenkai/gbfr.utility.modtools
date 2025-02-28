using DearImguiSharp;

using Reloaded.Mod.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gbfr.utility.modtools.ImGuiSupport.Windows;

public unsafe class AboutWindow : IImguiWindow, IImguiMenuComponent
{
    public bool IsOverlay => false;
    public bool IsOpen = false;

    private IModConfig _modConfig;

    public AboutWindow(IModConfig modConfig)
    {
        _modConfig = modConfig;
    }

    public void BeginMenuComponent()
    {
        if (ImGui.MenuItemEx("About Window", "", "", false, true))
        {
            IsOpen = true;
        }
    }

    public void Render(ImguiSupport imguiSupport)
    {
        if (!IsOpen)
            return;

        if (ImGui.Begin("Log Window", ref IsOpen, 0))
        {
            ImGui.Text($"{_modConfig.ModId} {_modConfig.ModVersion}");
            ImGui.Text($"Made by {_modConfig.ModAuthor}");
            ImGui.Spacing();

            ImGui.Text("Keys:");
            ImGui.Text("- INSERT: Show ImGui Menu");
            ImGui.Spacing();

            ImGui.Text("NOTE: Logs are also saved as a file in the game's directory as 'modtools_log.txt'.");
        }
    }
}
