using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using gbfr.utility.modtools.Hooks.Tables;

using NenTools.ImGui.Interfaces;

namespace gbfr.utility.modtools.ImGuiSupport.Windows.Tables;

public class WeaponManagerWindow : TableEditorWindow
{
    public WeaponManagerWindow(IImGui imGui, WeaponManagerHook tableManagerBase)
        : base(imGui, "WeaponManager", tableManagerBase)
    {

    }
}
