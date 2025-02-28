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

public unsafe class TableEditorWindow : IImguiWindow
{
    public bool IsOverlay => false;

    public string WindowName { get; set; }

    public DatabaseTable SelectedTable { get; set; }

    private bool _isOpen;
    private bool _groupRows;
    private bool _appliedColumnWidths = false;
    public float[] _colWidths;
    private TableManagerBase _tableManagerBase;

    public TableEditorWindow(string windowName, TableManagerBase managerBase)
    {
        WindowName = windowName;
        _tableManagerBase = managerBase;
    }

    public void BeginMenuComponent()
    {
        if (ImGui.MenuItemEx(WindowName, "", "", false, true))
            _isOpen = true;
    }

    public void OnSelectedTable()
    {
        _colWidths = new float[SelectedTable.Columns.Count];
        _appliedColumnWidths = false;
    }

    public void Render(ImguiSupport imguiSupport)
    {
        if (!_isOpen)
            return;

        if (ImGui.Begin(WindowName, ref _isOpen, 0))
        {
            var vecInternal = new ImVec2.__Internal();
            var vector = new ImVec2(&vecInternal); // Heap allocation

            if (ImGui.BeginCombo("Table", SelectedTable?.Name ?? "<select a table>", (int)ImGuiComboFlags.None))
            {
                foreach (var table in _tableManagerBase.Tables)
                {
                    bool isSelected = table == SelectedTable;
                    if (ImGui.SelectableBool(table.Name, isSelected, 0, vector))
                    {
                        SelectedTable = table;
                        OnSelectedTable();
                        isSelected = true;
                    }

                    if (isSelected)
                        ImGui.SetItemDefaultFocus();
                }

                ImGui.EndCombo();
            }

            ImGui.Checkbox("Group Rows by Key (for grouped tables)", ref _groupRows);
            ImGui.Spacing();

            RenderTable();

            ImGui.End();
        }
    }

    private void RenderTable()
    {
        if (SelectedTable is not null)
        {
            var vecInternal = new ImVec2.__Internal();
            var vector = new ImVec2(&vecInternal); // Heap allocation

            int numColumns = SelectedTable.IsVectorMap && _groupRows ? 1 : 1 + SelectedTable.Columns.Count;
            if (ImGui.BeginTable("#tbl", numColumns,
                (int)(ImGuiTableFlags.Borders | ImGuiTableFlags.ScrollX | ImGuiTableFlags.ScrollY | ImGuiTableFlags.RowBg | ImGuiTableFlags.Resizable),
                vector, 0.0f))
            {
                if (SelectedTable.RowMap is not null)
                    RenderTableFromUnorderedMap(SelectedTable.RowMap);
                else
                    RenderTableFromVector(SelectedTable.RowVector);

                ImGui.EndTable();
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
            ImGui.TableSetColumnWidth(1 + i, colWidth + 15);
        }

        _appliedColumnWidths = true;
    }

    private void RenderTableFromUnorderedMap(StdUnorderedMap* map)
    {
        var vecInternal = new ImVec2.__Internal();
        var vector = new ImVec2(&vecInternal); // Heap allocation

        if (_groupRows && SelectedTable.IsVectorMap)
        {
            ImGui.TableSetupColumn("Key", (int)ImGuiTableColumnFlags.WidthStretch, 5000.0f, 0);
            ImGui.TableHeadersRow();

            uint numRows = map->List.Size;
            StdListNode* currentEntry = map->List.Node->Next; // First entry is always empty

            for (int i = 0; i < numRows; i++)
            {
                ImGui.TableNextRow(0, 0);
                ImGui.TableNextColumn();

                StdVector* vec = (StdVector*)&currentEntry->Data;

                string idName = IdDatabase.Hashes.ContainsKey(currentEntry->Key) ? IdDatabase.Hashes[currentEntry->Key] : $"{currentEntry->Key:X8}";
                if (ImGui.TreeNodeExStr(idName, (int)ImGuiTreeNodeFlags.SpanFullWidth))
                {
                    if (ImGui.BeginTable("#tbl2", 1 + SelectedTable.Columns.Count,
                        (int)(ImGuiTableFlags.Borders | ImGuiTableFlags.ScrollX | ImGuiTableFlags.ScrollY | ImGuiTableFlags.RowBg | ImGuiTableFlags.Resizable),
                        vector, 0.0f))
                    {
                        RenderTableFromVector(vec);
                        ApplyColumnWidths();
                        ImGui.EndTable();
                    }

                    ImGui.TreePop();
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
        ImGui.TableSetupScrollFreeze(1, 1);
        ImGui.TableSetupColumn(string.Empty, 0, 0.0f, 0); // Row Number

        var vecInternal = new ImVec2.__Internal();
        var vector = new ImVec2(&vecInternal); // Heap allocation

        for (int i = 0; i < SelectedTable.Columns.Count; i++)
        {
            TableColumn? column = SelectedTable.Columns[i];
            if (!_appliedColumnWidths)
            {
                ImGui.CalcTextSize(vector, column.Name, null, false, 0.0f);
                _colWidths[i] = vector.X;
            }

            ImGui.TableSetupColumn(column.Name, 0, 0.0f, 0);
        }

        ImGui.TableHeadersRow();

    }

    private void AddRow(byte* rowData, int rowIndex)
    {
        ImGui.TableNextRow((int)ImGuiTableRowFlags.None, 0.0f);

        // Row number column for row
        ImGui.TableSetColumnIndex(0);
        ImGui.SetNextItemWidth(10);
        ImGui.Text(rowIndex.ToString());

        for (int j = 0; j < SelectedTable.Columns.Count; j++)
        {
            ImGui.TableSetColumnIndex(1 + j);
            ImGui.SetNextItemWidth(-1f); // Make the cell component fill the column

            var vecInternal = new ImVec2.__Internal();
            var vector = new ImVec2(&vecInternal); // Heap allocation

            byte* valPtr = rowData + SelectedTable.Columns[j].Offset;
            switch (SelectedTable.Columns[j].Type)
            {
                case DBColumnType.SByte:
                    if (!_appliedColumnWidths) ImGui.CalcTextSize(vector, (*(sbyte*)valPtr).ToString(), null, false, 0.0f);
                    ImGui.InputScalar($"##cell_{rowIndex}_{j}", (int)ImGuiDataType.S8, (nint)valPtr, 0, 0, "%d", (int)ImGuiInputTextFlags.None);
                    break;
                case DBColumnType.Byte:
                    if (!_appliedColumnWidths) ImGui.CalcTextSize(vector, (*valPtr).ToString(), null, false, 0.0f);
                    ImGui.InputScalar($"##cell_{rowIndex}_{j}", (int)ImGuiDataType.U8, (nint)valPtr, 0, 0, "%u", (int)ImGuiInputTextFlags.None);
                    break;
                case DBColumnType.Int:
                    if (!_appliedColumnWidths) ImGui.CalcTextSize(vector, (*(int*)valPtr).ToString(), null, false, 0.0f);
                    ImGui.InputScalar($"##cell_{rowIndex}_{j}", (int)ImGuiDataType.S32, (nint)valPtr, 0, 0, "%d", (int)ImGuiInputTextFlags.None);
                    break;
                case DBColumnType.UInt:
                    if (!_appliedColumnWidths) ImGui.CalcTextSize(vector, (*(uint*)valPtr).ToString(), null, false, 0.0f);
                    ImGui.InputScalar($"##cell_{rowIndex}_{j}", (int)ImGuiDataType.U32, (nint)valPtr, 0, 0, "%u", (int)ImGuiInputTextFlags.None);
                    break;
                case DBColumnType.Short:
                    if (!_appliedColumnWidths) ImGui.CalcTextSize(vector, (*(short*)valPtr).ToString(), null, false, 0.0f);
                    ImGui.InputScalar($"##cell_{rowIndex}_{j}", (int)ImGuiDataType.S16, (nint)valPtr, 0, 0, "%d", (int)ImGuiInputTextFlags.None);
                    break;
                case DBColumnType.Int64:
                    if (!_appliedColumnWidths) ImGui.CalcTextSize(vector, (*(long*)valPtr).ToString(), null, false, 0.0f);
                    ImGui.InputScalar($"##cell_{rowIndex}_{j}", (int)ImGuiDataType.S64, (nint)valPtr, 0, 0, "%d", (int)ImGuiInputTextFlags.None);
                    break;
                case DBColumnType.HexUInt:
                    if (!_appliedColumnWidths) ImGui.CalcTextSize(vector, (*(uint*)valPtr).ToString("X8"), null, false, 0.0f);
                    ImGui.InputScalar($"##cell_{rowIndex}_{j}", (int)ImGuiDataType.U32, (nint)valPtr, 0, 0, "%x", (int)ImGuiInputTextFlags.None);
                    break;

                case DBColumnType.Float:
                    if (!_appliedColumnWidths) ImGui.CalcTextSize(vector, (*(float*)valPtr).ToString(), null, false, 0.0f);
                    ImGui.InputScalar($"##cell_{rowIndex}_{j}", (int)ImGuiDataType.Float, (nint)valPtr, 0, 0, "%0.2f", (int)ImGuiInputTextFlags.None);
                    break;

                case DBColumnType.Double:
                    if (!_appliedColumnWidths) ImGui.CalcTextSize(vector, (*(double*)valPtr).ToString(), null, false, 0.0f);
                    ImGui.InputScalar($"##cell_{rowIndex}_{j}", (int)ImGuiDataType.Double, (nint)valPtr, 0, 0, "%d", (int)ImGuiInputTextFlags.None);
                    break;

                case DBColumnType.HashString:

                    if (IdDatabase.Hashes.TryGetValue(*(uint*)valPtr, out string id))
                    {
                        nint strPtr = Marshal.StringToHGlobalAnsi(id);

                        if (!_appliedColumnWidths) ImGui.CalcTextSize(vector, id, null, false, 0.0f);
                        ImGui.InputText($"##cell_{rowIndex}_{j}", (sbyte*)strPtr, id.Length + 1, (int)ImGuiInputTextFlags.None, null, 0);
                    }
                    else
                    {
                        string idHex = (*(uint*)valPtr).ToString("X8");
                        nint strPtr = Marshal.StringToHGlobalAnsi(idHex);

                        if (!_appliedColumnWidths) ImGui.CalcTextSize(vector, idHex, null, false, 0.0f);
                        ImGui.InputText($"##cell_{rowIndex}_{j}", (sbyte*)strPtr, 9, (int)ImGuiInputTextFlags.None, null, 0);
                    }
                    break;

                case DBColumnType.RawString:

                    if (!_appliedColumnWidths)
                    {
                        string str = Encoding.UTF8.GetString(valPtr, SelectedTable.Columns[j].StringLength);
                        ImGui.CalcTextSize(vector, str, null, false, 0.0f);
                    }
                    ImGui.InputText($"##cell_{rowIndex}_{j}", (sbyte*)valPtr, SelectedTable.Columns[j].StringLength, (int)ImGuiInputTextFlags.None, null, 0);
                    break;

                default:
                    break;

            }

            if (!_appliedColumnWidths && vector.X > _colWidths[j])
                _colWidths[j] = vector.X;
        }
    }
}
