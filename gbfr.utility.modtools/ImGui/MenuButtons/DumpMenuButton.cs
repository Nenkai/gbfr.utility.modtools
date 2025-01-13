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

public unsafe class DumpMenuButton : IImguiMenuComponent
{
    private ReflectionHooks _reflectionHooks;

    public DumpMenuButton(ReflectionHooks refHooks)
    {
        _reflectionHooks = refHooks;
    }

    public void BeginMenuComponent()
    {
        if (ImGui.MenuItemEx($"Dump reflection classes ({_reflectionHooks.ObjectCount})", "", "", false, _reflectionHooks.HasLoadedObjects))
        {
            _reflectionHooks.DumpAll();
        }
    }
}
