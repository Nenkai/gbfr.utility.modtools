using DearImguiSharp;

using gbfr.utility.modtools.Hooks.Effects;

using Reloaded.Mod.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gbfr.utility.modtools.ImGuiSupport.Windows;

public unsafe class EffectEditWindow : IImguiWindow, IImguiMenuComponent
{
    public bool IsOverlay => false;
    public bool IsOpen = false;

    private EffectDataHooks _effectDataHooks;

    public EffectEditWindow(EffectDataHooks effectDataHooks)
    {
        _effectDataHooks = effectDataHooks;
    }

    public void BeginMenuComponent()
    {
        if (ImGui.MenuItemEx("About Window", "", "", false, true))
        {
            IsOpen = true;
        }
    }

    private EstFile _selectedEffectData;

    public void Render()
    {
        if (!IsOpen)
            return;

        if (ImGui.Begin("Effect Edit", ref IsOpen, 0))
        {
            var availRegionVecInt = new ImVec2.__Internal();
            var availRegionVec = new ImVec2(&availRegionVecInt);
            ImGui.GetContentRegionAvail(availRegionVec);

            // Make effect list
            var vecInternal = new ImVec2.__Internal();
            vecInternal.x = 250;
            vecInternal.y = availRegionVec.Y;
            var vector = new ImVec2(&vecInternal);

            ImGui.BeginChildStr("EffectListL", vector, false, 0);
            bool visible = true;
            foreach (var effSet in _effectDataHooks.EffectSets)
            {
                if (ImGui.CollapsingHeaderBoolPtr(effSet.Key, ref visible, 0))
                {
                    var listboxVec_ = new ImVec2.__Internal();
                    var listboxVec = new ImVec2(&listboxVec_);

                    if (ImGui.BeginListBox("##Listbox1", listboxVec))
                    {
                        foreach (var eff in effSet.Value.EffectIds)
                        {
                            var vecInternal_ = new ImVec2.__Internal();
                            var vector_ = new ImVec2(&vecInternal_);

                            if (ImGui.SelectableBool(eff.Key.ToString(), false, 0, vector_))
                            {
                                _selectedEffectData = eff.Value;
                            }
                        }

                        ImGui.EndListBox();
                    }
                }
            }
            ImGui.EndChild();

            var effectEditorRVec_ = new ImVec2.__Internal();
            vecInternal.y = availRegionVec.Y;
            var effectEditorRVec = new ImVec2(&effectEditorRVec_);

            ImGui.SameLine(0, 4);

            ImGui.BeginChildStr("EffectEditorR", effectEditorRVec, false, 0);
            
            if (_selectedEffectData is not null)
            {
                ImGui.Text($"{_selectedEffectData.Id} (est: 0x{_selectedEffectData.FilePointer:X8})");

                sEstHeader* estHeader = (sEstHeader*)_selectedEffectData.FilePointer;
                ImGui.Text($"NumEntries: {estHeader->NumEntries}");
                ImGui.Text($"EntryArrayMapOffset: 0x{estHeader->EntryArrayMapOffset:X}");
                ImGui.Text($"OffsetOfFunctions: 0x{estHeader->OffsetOfFunctions:X}");
                ImGui.Text($"EntryDataOffsetStart: 0x{estHeader->EntryDataOffsetStart:X}");
                ImGui.Text($"FunctionSize: 0x{estHeader->FunctionSize:X}");
                ImGui.Text($"NumFunctionsPerTable: {estHeader->NumFunctionsPerTable}");
            }
            else
            {
                ImGui.Text($"No EST Selected");
            }
        }
    }
}
