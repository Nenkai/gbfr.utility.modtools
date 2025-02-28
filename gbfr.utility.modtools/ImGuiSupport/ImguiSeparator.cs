using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using DearImguiSharp;

using gbfr.utility.modtools.Hooks;

namespace gbfr.utility.modtools.ImGuiSupport;

public class ImguiSeparator : IImguiMenuComponent
{
    public ImguiSeparator()
    {
        
    }

    public void BeginMenuComponent()
    {
        ImGui.Separator();
    }
}
