using gbfr.utility.modtools.Hooks.Reflection;
using gbfr.utility.modtools.ImGuiSupport;

using NenTools.ImGui.Interfaces;
using NenTools.ImGui.Interfaces.Shell;

namespace gbfr.utility.modtools.ImGuiSupport.MenuButtons;

public unsafe class DumpMenuButton : IImGuiComponent
{
    private readonly IImGui _imGui;
    private readonly ReflectionHooks _reflectionHooks;

    public DumpMenuButton(IImGui imgui, ReflectionHooks reflectionHooks)
    {
        _imGui = imgui;
        _reflectionHooks = reflectionHooks;
    }

    public bool IsOverlay => false;

    public void Render(IImGuiShell imGuiShell)
    {

    }

    public void RenderMenu(IImGuiShell imGuiShell)
    {
        if (_imGui.MenuItemEx($"Dump reflection classes ({_reflectionHooks.ObjectCount})", "", false, _reflectionHooks.HasLoadedObjects))
        {
            _reflectionHooks.DumpAll();
        }
    }
}
