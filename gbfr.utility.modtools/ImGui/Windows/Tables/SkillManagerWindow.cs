using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using DearImguiSharp;

using GBFRDataTools.Database;
using GBFRDataTools.Database.Entities;
using gbfr.utility.modtools.Hooks.Managers;

namespace gbfr.utility.modtools.ImGuiSupport.Windows.Tables;

public class SkillManagerWindow : TableEditorWindow
{
    public SkillManagerWindow(TableManagerBase tableManagerBase)
        : base("SkillManager", tableManagerBase)
    {

    }
}
