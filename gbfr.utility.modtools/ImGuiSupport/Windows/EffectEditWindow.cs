using gbfr.utility.modtools.Hooks.Effects;

using NenTools.ImGui.Interfaces;
using NenTools.ImGui.Interfaces.Shell;

using Reloaded.Mod.Interfaces;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace gbfr.utility.modtools.ImGuiSupport.Windows;

public unsafe class EffectEditWindow : IImGuiComponent
{
    private readonly IImGui _imGui;
    private readonly EffectDataHooks _effectDataHooks;

    public bool IsOverlay => false;
    public bool IsOpen = false;

    private EstFile _selectedEffectData;
    private int _selectedTableIndex = -1;

    public EffectEditWindow(IImGui imGui, EffectDataHooks effectDataHooks)
    {
        _imGui = imGui;
        _effectDataHooks = effectDataHooks;
    }

    public void RenderMenu(IImGuiShell imGuiShell)
    {
        if (_imGui.MenuItemEx("Loaded Effects"u8, ""u8, false, true))
        {
            IsOpen = true;
        }
    }

    public void Render(IImGuiShell imGuiShell)
    {
        if (!IsOpen)
            return;

        if (_imGui.Begin("Effect Edit"u8, ref IsOpen))
        {
            Vector2 availRegionVec = _imGui.GetContentRegionAvail();

            // Make effect list

            _imGui.BeginChild("EffectListL"u8, new Vector2(250, availRegionVec.Y));
            bool visible = true;
            foreach (var effSet in _effectDataHooks.EffectSets)
            {
                if (_imGui.CollapsingHeaderBoolPtr(effSet.Key, ref visible, 0))
                {
                    if (_imGui.BeginListBox("##Listbox1"u8, Vector2.Zero))
                    {
                        foreach (var eff in effSet.Value.EffectIds)
                        {
                            if (_imGui.Selectable(eff.Key.ToString()))
                            {
                                _selectedEffectData = eff.Value;
                                _selectedTableIndex = -1;
                            }
                        }

                        _imGui.EndListBox();
                    }
                }
            }
            _imGui.EndChild();

            _imGui.SameLineEx(0, 4);

            _imGui.BeginChild("EffectEditorR"u8, Vector2.Zero);

            if (_selectedEffectData is not null && *(uint*)_selectedEffectData.FilePointer != 0x00464645)
            {
                _selectedEffectData = null;
                _selectedTableIndex = -1;
            }

            if (_selectedEffectData is not null)
            {
                _imGui.Text($"{_selectedEffectData.Id} (est: 0x{_selectedEffectData.FilePointer:X8}) ");
                _imGui.Separator();

                if (true)
                {

                    sEstHeader* estHeader = (sEstHeader*)_selectedEffectData.FilePointer;
                    _imGui.Text($"NumEntries: {estHeader->NumEntries}");
                    _imGui.Text($"EntryArrayMapOffset: 0x{estHeader->EntryArrayMapOffset:X}");
                    _imGui.Text($"OffsetOfFunctions: 0x{estHeader->OffsetOfFunctions:X}");
                    _imGui.Text($"EntryDataOffsetStart: 0x{estHeader->EntryDataOffsetStart:X}");
                    _imGui.Text($"FunctionSize: 0x{estHeader->FunctionSize:X}");
                    _imGui.Text($"NumFunctionsPerTable: {estHeader->NumFunctionsPerTable}");


                    if (_imGui.BeginCombo($"Entries", _selectedTableIndex == -1 ? "Select Table..." : $"Table #{_selectedTableIndex}", 0))
                    {
                        for (int i = 0; i < estHeader->NumEntries; i++)
                        {
                            if (_imGui.Selectable($"Table #{i}"))
                            {
                                _selectedTableIndex = i;
                            }

                            if (i == _selectedTableIndex)
                                _imGui.SetItemDefaultFocus();
                        }
                        _imGui.EndCombo();
                    }

                    if (_selectedTableIndex != -1)
                    {
                        int* entriesOffsets = (int*)((byte*)estHeader + estHeader->EntryArrayMapOffset);
                        byte* entryOffset = (byte*)estHeader + entriesOffsets[_selectedTableIndex];

                        EstFunction* functions = (EstFunction*)((byte*)estHeader + estHeader->OffsetOfFunctions);
                        EstFunction* tableFuncs = &functions[_selectedTableIndex * estHeader->NumFunctionsPerTable];

                        for (int j = 0; j < estHeader->NumFunctionsPerTable; j++)
                        {
                            _imGui.BeginDisabled(tableFuncs[j].Size != 0);
                            if (_imGui.Button($"{Encoding.ASCII.GetString(BitConverter.GetBytes(tableFuncs[j].FuncName))}"))
                            {

                            }

                            _imGui.EndDisabled();

                            if (j != estHeader->NumFunctionsPerTable - 1)
                                _imGui.SameLineEx(0, 2);
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
                _imGui.Text("No EST Selected"u8);
            }

            _imGui.EndChild();
        }
        _imGui.End();
    }
}
