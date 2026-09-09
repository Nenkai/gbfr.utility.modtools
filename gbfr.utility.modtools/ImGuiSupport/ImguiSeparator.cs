using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using NenTools.ImGui.Interfaces;
using NenTools.ImGui.Interfaces.Shell;

namespace gbfr.utility.modtools.ImGuiSupport;

public class ImguiSeparator : IImGuiComponent
{
    private readonly IImGui _imGui;

    public ImguiSeparator(IImGui imgui)
    {
        _imGui = imgui;
    }

    public bool IsOverlay => false;

    public void Render(IImGuiShell imGuiShell)
    {
        
    }

    public void RenderMenu(IImGuiShell imGuiShell)
    {
        _imGui.Separator();
    }
}
