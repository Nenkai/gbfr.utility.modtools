using DearImguiSharp;

using gbfr.utility.modtools.Hooks.Effects;

using Reloaded.Mod.Interfaces;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

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
    private int _selectedTableIndex = -1;

    public void Render(ImguiSupport imguiSupport)
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
                                _selectedTableIndex = -1;
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

            if (_selectedEffectData is not null && *(uint*)_selectedEffectData.FilePointer != 0x00464645)
            {
                _selectedEffectData = null;
                _selectedTableIndex = -1;
            }

            if (_selectedEffectData is not null)
            {
                ImGui.Text($"{_selectedEffectData.Id} (est: 0x{_selectedEffectData.FilePointer:X8}) ");
                ImGui.Separator();

                if (true)
                {

                    sEstHeader* estHeader = (sEstHeader*)_selectedEffectData.FilePointer;
                    ImGui.Text($"NumEntries: {estHeader->NumEntries}");
                    ImGui.Text($"EntryArrayMapOffset: 0x{estHeader->EntryArrayMapOffset:X}");
                    ImGui.Text($"OffsetOfFunctions: 0x{estHeader->OffsetOfFunctions:X}");
                    ImGui.Text($"EntryDataOffsetStart: 0x{estHeader->EntryDataOffsetStart:X}");
                    ImGui.Text($"FunctionSize: 0x{estHeader->FunctionSize:X}");
                    ImGui.Text($"NumFunctionsPerTable: {estHeader->NumFunctionsPerTable}");


                    if (ImGui.BeginCombo($"Entries", _selectedTableIndex == -1 ? "Select Table..." : $"Table #{_selectedTableIndex}", 0))
                    {
                        for (int i = 0; i < estHeader->NumEntries; i++)
                        {
                            var cb_ = new ImVec2.__Internal();
                            var cb = new ImVec2(&cb_);

                            if (ImGui.SelectableBool($"Table #{i}", false, 0, cb))
                            {
                                _selectedTableIndex = i;
                            }

                            if (i == _selectedTableIndex)
                                ImGui.SetItemDefaultFocus();
                        }
                        ImGui.EndCombo();
                    }

                    if (_selectedTableIndex != -1)
                    {
                        int* entriesOffsets = (int*)((byte*)estHeader + estHeader->EntryArrayMapOffset);
                        byte* entryOffset = (byte*)estHeader + entriesOffsets[_selectedTableIndex];

                        EstFunction* functions = (EstFunction*)((byte*)estHeader + estHeader->OffsetOfFunctions);
                        EstFunction* tableFuncs = &functions[_selectedTableIndex * estHeader->NumFunctionsPerTable];

                        for (int j = 0; j < estHeader->NumFunctionsPerTable; j++)
                        {
                            var btn_ = new ImVec2.__Internal();
                            var btn = new ImVec2(&btn_);

                            ImGui.BeginDisabled(tableFuncs[j].Size != 0);
                            if (ImGui.Button($"{Encoding.ASCII.GetString(BitConverter.GetBytes(tableFuncs[j].FuncName))}", btn))
                            {

                            }

                            ImGui.EndDisabled();

                            if (j != estHeader->NumFunctionsPerTable - 1)
                                ImGui.SameLine(0, 2);
                        }

                        for (int j = 0; j < estHeader->NumFunctionsPerTable; j++)
                        {
                            if (tableFuncs[j].Size != 0 && (tableFuncs[j].FuncName == 0x41535A53 || tableFuncs[j].FuncName == 0x41534D45))
                            {
                                EstSizeSinAnm* dataPtr = (EstSizeSinAnm*)(entryOffset + tableFuncs[j].Offset);

                            }
                        }
                    }
                }
            }
            else
            {
                ImGui.Text($"No EST Selected");
            }

            ImGui.EndChild();
        }
    }
}
