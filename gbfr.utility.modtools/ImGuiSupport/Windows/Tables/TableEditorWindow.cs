using gbfr.utility.modtools.Hooks.Tables;
using gbfr.utility.modtools.Native;

using GBFRDataTools.Database;
using GBFRDataTools.Database.Entities;

using NenTools.ImGui.Implementation;
using NenTools.ImGui.Interfaces;
using NenTools.ImGui.Interfaces.Shell;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace gbfr.utility.modtools.ImGuiSupport.Windows.Tables;

public unsafe class TableEditorWindow : IImGuiComponent
{
    private readonly IImGui _imGui;

    public bool IsOverlay => false;

    public string WindowName { get; set; }

    public DatabaseTable SelectedTable { get; set; }

    private bool _isOpen;
    private bool _groupRows;
    private bool _appliedColumnWidths = false;
    public float[] _colWidths;
    private TableManagerBase _tableManagerBase;

    public TableEditorWindow(IImGui imGui, string windowName, TableManagerBase managerBase)
    {
        _imGui = imGui;

        WindowName = windowName;
        _tableManagerBase = managerBase;
    }

    public void RenderMenu(IImGuiShell imGuiShell)
    {
        if (_imGui.MenuItemEx(WindowName, "", false, true))
            _isOpen = true;
    }

    public void OnSelectedTable()
    {
        _colWidths = new float[SelectedTable.Columns.Count];
        _appliedColumnWidths = false;
    }

    public void Render(IImGuiShell imGuiShell)
    {
        if (!_isOpen)
            return;

        if (_imGui.Begin(WindowName, ref _isOpen))
        {
            if (_imGui.BeginCombo("Table", SelectedTable?.Name ?? "<select a table>"))
            {
                foreach (var table in _tableManagerBase.Tables)
                {
                    bool isSelected = table == SelectedTable;
                    if (_imGui.SelectableEx(table.Name, isSelected, 0, Vector2.Zero))
                    {
                        SelectedTable = table;
                        OnSelectedTable();
                        isSelected = true;
                    }

                    if (isSelected)
                        _imGui.SetItemDefaultFocus();
                }

                _imGui.EndCombo();
            }

            _imGui.Checkbox("Group Rows by Key (for grouped tables)", ref _groupRows);
            _imGui.Spacing();

            RenderTable();

            _imGui.End();
        }
    }

    private void RenderTable()
    {
        if (SelectedTable is not null)
        {
            int numColumns = SelectedTable.IsVectorMap && _groupRows ? 1 : 1 + SelectedTable.Columns.Count;
            if (_imGui.BeginTable("#tbl"u8, numColumns, ImGuiTableFlags.ImGuiTableFlags_Borders | ImGuiTableFlags.ImGuiTableFlags_ScrollX | ImGuiTableFlags.ImGuiTableFlags_ScrollY | 
                ImGuiTableFlags.ImGuiTableFlags_RowBg | ImGuiTableFlags.ImGuiTableFlags_Resizable))
            {
                if (SelectedTable.RowMap is not null)
                    RenderTableFromUnorderedMap(SelectedTable.RowMap);
                else
                    RenderTableFromVector(SelectedTable.RowVector);

                _imGui.EndTable();
            }
        }
    }

    private void RenderTableFromVector(StdVector* vector)
    {
        SetupTableHeader();

        int listSize = (int)(((byte*)vector->Mylast - (byte*)vector->Myfirst) / sizeof(ulong*));
        for (int j = 0; j < listSize; j++)
        {
            AddRow((byte*)((ulong*)vector->Myfirst)[j], j);
        }
    }

    private void ApplyColumnWidths()
    {
        if (_appliedColumnWidths)
            return;

        for (int i = 0; i < _colWidths.Length; i++)
        {
            float colWidth = _colWidths[i];
            _imGui.SetColumnWidth(1 + i, colWidth + 15); // TODO: Fix
        }

        _appliedColumnWidths = true;
    }

    private void RenderTableFromUnorderedMap(StdUnorderedMap* map)
    {
        if (_groupRows && SelectedTable.IsVectorMap)
        {
            _imGui.TableSetupColumnEx("Key", ImGuiTableColumnFlags.ImGuiTableColumnFlags_WidthStretch, 5000.0f, 0);
            _imGui.TableHeadersRow();

            uint numRows = map->List.Size;
            StdListNode* currentEntry = map->List.Node->Next; // First entry is always empty

            for (int i = 0; i < numRows; i++)
            {
                _imGui.TableNextRow();
                _imGui.TableNextColumn();

                StdVector* vec = (StdVector*)&currentEntry->Data;

                string idName = IdDatabase.Hashes.ContainsKey(currentEntry->Key) ? IdDatabase.Hashes[currentEntry->Key] : $"{currentEntry->Key:X8}";
                if (_imGui.TreeNodeEx(idName, ImGuiTreeNodeFlags.ImGuiTreeNodeFlags_SpanFullWidth))
                {
                    if (_imGui.BeginTable("#tbl2", 1 + SelectedTable.Columns.Count, 
                        ImGuiTableFlags.ImGuiTableFlags_Borders | ImGuiTableFlags.ImGuiTableFlags_ScrollX | ImGuiTableFlags.ImGuiTableFlags_ScrollY | ImGuiTableFlags.ImGuiTableFlags_RowBg | ImGuiTableFlags.ImGuiTableFlags_Resizable))
                    {
                        RenderTableFromVector(vec);
                        ApplyColumnWidths();
                        _imGui.EndTable();
                    }

                    _imGui.TreePop();
                }

                currentEntry = currentEntry->Next;
            }
        }
        else
        {
            SetupTableHeader();

            StdListNode* currentEntry = map->List.Node->Next; // First entry is always empty

            if (SelectedTable.IsVectorMap)
            {
                // Flatten, since we're not grouping
                uint numGroups = map->List.Size;

                int rowIndex = 0;
                for (int i = 0; i < numGroups; i++)
                {
                    StdVector* vec = (StdVector*)&currentEntry->Data;

                    int listSize = (int)(((byte*)vec->Mylast - (byte*)vec->Myfirst) / sizeof(ulong*));
                    for (int j = 0; j < listSize; j++)
                    {
                        AddRow((byte*)*((ulong*)vec->Myfirst + j), rowIndex++);
                    }

                    currentEntry = currentEntry->Next;
                }
            }
            else
            {
                for (int i = 0; i < map->List.Size; i++)
                {
                    AddRow((byte*)currentEntry->Data, i);
                    currentEntry = currentEntry->Next;
                }
            }
        }

        ApplyColumnWidths();
    }

    private void SetupTableHeader()
    {
        _imGui.TableSetupScrollFreeze(1, 1);
        _imGui.TableSetupColumn(string.Empty); // Row Number

        for (int i = 0; i < SelectedTable.Columns.Count; i++)
        {
            TableColumn column = SelectedTable.Columns[i];
            if (!_appliedColumnWidths)
            {
                Vector2 textSize = _imGui.CalcTextSize(column.Name);
                _colWidths[i] = textSize.X;
            }

            _imGui.TableSetupColumn(column.Name);
        }

        _imGui.TableHeadersRow();

    }

    private void AddRow(byte* rowData, int rowIndex)
    {
        _imGui.TableNextRow();

        // Row number column for row
        _imGui.TableSetColumnIndex(0);
        _imGui.SetNextItemWidth(10);
        _imGui.Text(rowIndex.ToString());

        for (int j = 0; j < SelectedTable.Columns.Count; j++)
        {
            _imGui.TableSetColumnIndex(1 + j);
            _imGui.SetNextItemWidth(-1f); // Make the cell component fill the column

            byte* valPtr = rowData + SelectedTable.Columns[j].Offset;
            Vector2 size = Vector2.Zero;
            switch (SelectedTable.Columns[j].Type)
            {
                case DBColumnType.SByte:
                    if (!_appliedColumnWidths) size = _imGui.CalcTextSize((*(sbyte*)valPtr).ToString());
                    _imGui.InputScalar($"##cell_{rowIndex}_{j}", ref Unsafe.AsRef<sbyte>(valPtr));
                    break;
                case DBColumnType.Byte:
                    if (!_appliedColumnWidths) size = _imGui.CalcTextSize((*valPtr).ToString());
                    _imGui.InputScalar($"##cell_{rowIndex}_{j}", ref Unsafe.AsRef<byte>(valPtr));
                    break;
                case DBColumnType.Int:
                    if (!_appliedColumnWidths) size = _imGui.CalcTextSize((*(int*)valPtr).ToString());
                    _imGui.InputScalar($"##cell_{rowIndex}_{j}", ref Unsafe.AsRef<int>(valPtr));
                    break;
                case DBColumnType.UInt:
                case DBColumnType.HexUInt:
                    if (!_appliedColumnWidths) size = _imGui.CalcTextSize((*(uint*)valPtr).ToString());
                    _imGui.InputScalar($"##cell_{rowIndex}_{j}", ref Unsafe.AsRef<uint>(valPtr));
                    break;
                case DBColumnType.Short:
                    if (!_appliedColumnWidths) size = _imGui.CalcTextSize((*(short*)valPtr).ToString());
                    _imGui.InputScalar($"##cell_{rowIndex}_{j}", ref Unsafe.AsRef<short>(valPtr));
                    break;
                case DBColumnType.Int64:
                    if (!_appliedColumnWidths) size = _imGui.CalcTextSize((*(long*)valPtr).ToString());
                    _imGui.InputScalar($"##cell_{rowIndex}_{j}", ref Unsafe.AsRef<long>(valPtr));
                    break;
                case DBColumnType.Float:
                    if (!_appliedColumnWidths) size = _imGui.CalcTextSize((*(float*)valPtr).ToString());
                    _imGui.InputScalar($"##cell_{rowIndex}_{j}", ref Unsafe.AsRef<float>(valPtr));
                    break;

                case DBColumnType.Double:
                    if (!_appliedColumnWidths) size = _imGui.CalcTextSize((*(double*)valPtr).ToString());
                    _imGui.InputScalar($"##cell_{rowIndex}_{j}", ref Unsafe.AsRef<double>(valPtr));
                    break;

                case DBColumnType.HashString:

                    if (IdDatabase.Hashes.TryGetValue(*(uint*)valPtr, out string id))
                    {
                        nint strPtr = Marshal.StringToHGlobalAnsi(id);

                        if (!_appliedColumnWidths) size = _imGui.CalcTextSize(id);
                        _imGui.InputText($"##cell_{rowIndex}_{j}", (sbyte*)strPtr, (nuint)id.Length + 1);
                    }
                    else
                    {
                        string idHex = (*(uint*)valPtr).ToString("X8");
                        nint strPtr = Marshal.StringToHGlobalAnsi(idHex);

                        if (!_appliedColumnWidths) size = _imGui.CalcTextSize(idHex);
                        _imGui.InputText($"##cell_{rowIndex}_{j}", (sbyte*)strPtr, 9);
                    }
                    break;

                case DBColumnType.RawString:

                    if (!_appliedColumnWidths)
                    {
                        string str = Encoding.UTF8.GetString(valPtr, SelectedTable.Columns[j].StringLength);
                        size = _imGui.CalcTextSize(str);
                    }
                    _imGui.InputText($"##cell_{rowIndex}_{j}", (sbyte*)valPtr, (nuint)SelectedTable.Columns[j].StringLength);
                    break;

                default:
                    break;

            }

            if (!_appliedColumnWidths && size.X > _colWidths[j])
                _colWidths[j] = size.X;
        }
    }
}
