using gbfr.utility.modtools.Hooks.Tables;

using NenTools.ImGui.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace gbfr.utility.modtools.ImGuiSupport.Windows.Tables;

public class SkillManagerWindow : TableEditorWindow
{
    public SkillManagerWindow(IImGui imGui, SkillManagerHook tableManagerBase)
        : base(imGui, "SkillManager", tableManagerBase)
    {

    }
}
