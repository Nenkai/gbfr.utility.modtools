using DearImguiSharp;

using Reloaded.Mod.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gbfr.utility.modtools.ImGuiSupport.Windows;

public unsafe class DemoWindow : IImguiWindow, IImguiMenuComponent
{
    public bool IsOverlay => false;
    public bool IsOpen = false;

    public DemoWindow()
    {
        
    }

    public void BeginMenuComponent()
    {
        if (ImGui.MenuItemEx("ImGui Demo Window", "", "", false, true))
        {
            IsOpen = true;
        }
    }

    public void Render(ImguiSupport imguiSupport)
    {
        if (!IsOpen)
            return;

        ImGui.ShowDemoWindow(ref IsOpen);
    }
}
