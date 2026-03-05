using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using DearImguiSharp;

using gbfr.utility.modtools.Hooks.Tables;

namespace gbfr.utility.modtools.ImGuiSupport.Windows.Tables;

public class SkillManagerWindow : TableEditorWindow
{
    public SkillManagerWindow(SkillManagerHook tableManagerBase)
        : base("SkillManager", tableManagerBase)
    {

    }
}
