using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using DearImguiSharp;

using gbfr.utility.modtools.Hooks;
using gbfr.utility.modtools.ImGuiSupport;

namespace gbfr.utility.modtools.ImGuiSupport.MenuButtons;

public unsafe class MouseControlButton : IImguiMenuComponent
{
    private ImguiSupport _imguiSupport;

    public MouseControlButton(ImguiSupport imguiSupport)
    {
        _imguiSupport = imguiSupport;
    }

    public void BeginMenuComponent()
    {
        ImGui.MenuItemBoolPtr($"Enable mouse control while menu is active", "", ref _imguiSupport.MouseActiveWhileMenuOpen, true);
    }
}
