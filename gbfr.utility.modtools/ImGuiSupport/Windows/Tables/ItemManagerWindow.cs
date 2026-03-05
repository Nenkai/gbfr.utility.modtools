using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using gbfr.utility.modtools.Hooks.Tables;

namespace gbfr.utility.modtools.ImGuiSupport.Windows.Tables;

public class ItemManagerWindow : TableEditorWindow
{
    public ItemManagerWindow(ItemManagerHook tableManagerBase)
        : base("ItemManager", tableManagerBase)
    {

    }
}
