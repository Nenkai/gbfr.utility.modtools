using gbfr.utility.modtools.Hooks;
using gbfr.utility.modtools.ImGuiSupport;

using NenTools.ImGui.Interfaces;
using NenTools.ImGui.Interfaces.Shell;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace gbfr.utility.modtools.ImGuiSupport.MenuButtons;

public class MouseControlButton : IImGuiComponent
{
    private readonly IImGui _imGui;
    private readonly ImGuiInputHookManager _imguiSupport;

    public MouseControlButton(IImGui imGui, ImGuiInputHookManager imguiSupport)
    {
        _imGui = imGui;
        _imguiSupport = imguiSupport;
    }

    public bool IsOverlay => false;

    public void Render(IImGuiShell imGuiShell)
    {
        
    }

    public void RenderMenu(IImGuiShell imGuiShell)
    {
        _imGui.MenuItemBoolPtr($"Enable mouse control while menu is active", "", ref _imguiSupport.MouseActiveWhileMenuOpen, true);
    }
}
