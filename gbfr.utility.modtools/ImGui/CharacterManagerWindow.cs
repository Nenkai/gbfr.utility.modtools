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

namespace gbfr.utility.modtools.Windows;

public unsafe class CharacterManagerWindow
{
    public static bool IsOpen;

    public static List<DatabaseTable> Tables { get; set; } = new();
    public static DatabaseTable SelectedTable { get; set; }

    public static void AddTable(string name, StdUnorderedMap* rows, bool isVectorMap = false)
    {
        List<TableColumn> columns = TableMappingReader.ReadColumnMappings(name, new Version(1, 3, 1), out int readSize);

        var table = new DatabaseTable(name, columns, readSize, rows, isVectorMap);
        Tables.Add(table);

        SelectedTable ??= table;
    }

    public static void Render()
    {
        if (!IsOpen)
            return;

        if (ImGui.Begin("CharacterManager", ref IsOpen, 0))
        {
            var vecInternal = new ImVec2.__Internal();
            var vector = new ImVec2(&vecInternal); // Heap allocation

            if (ImGui.BeginCombo("Table", SelectedTable.Name, (int)ImGuiComboFlags.None))
            {
                foreach (var table in Tables)
                {
                    bool isSelected = table == SelectedTable;
                    if (ImGui.SelectableBool(table.Name, isSelected, 0, vector))
                    {
                        SelectedTable = table;
                        isSelected = true;
                    }

                    if (isSelected)
                        ImGui.SetItemDefaultFocus();
                }

                ImGui.EndCombo();
            }

            ImGui.Spacing();

            int numColumns = 1 + SelectedTable.Columns.Count;
            if (ImGui.BeginTable("#tbl", numColumns, 
                (int)(ImGuiTableFlags.Borders | ImGuiTableFlags.Resizable | ImGuiTableFlags.ScrollX | ImGuiTableFlags.ScrollY |  ImGuiTableFlags.RowBg),
                vector, 0.0f))
            {
                ImGui.TableSetupScrollFreeze(1, 1);
                ImGui.TableSetupColumn(string.Empty, (int)ImGuiTableColumnFlags.None, 0.0f, 0); // Row Number

                foreach (var column in SelectedTable.Columns)
                    ImGui.TableSetupColumn(column.Name, (int)ImGuiTableColumnFlags.None, 0.0f, 0);
                ImGui.TableHeadersRow();

                StdListNode* currentEntry = SelectedTable.Rows->List.Node->Next; // First entry is always empty

                if (SelectedTable.IsVectorMap)
                {
                    uint numLists = SelectedTable.Rows->List.Size;

                    int rowIndex = 0;
                    for (int i = 0; i < numLists; i++)
                    {
                        StdVector* vec = (StdVector*)&currentEntry->Data;

                        int listSize = (int)(((byte*)vec->Myend - (byte*)vec->Myfirst) / sizeof(ulong*));
                        for (int j = 0; j < listSize; j++)
                        {
                            AddRow((byte*)*((ulong*)vec->Myfirst + j), rowIndex++);
                        }

                        currentEntry = currentEntry->Next;
                    }
                }
                else
                {
                    for (int i = 0; i < SelectedTable.Rows->List.Size; i++)
                    {
                        AddRow((byte*)currentEntry->Data, i);
                        currentEntry = currentEntry->Next;
                    }
                }

                ImGui.EndTable();
            }

            ImGui.End();
        }
    }

    private static void AddRow(byte* rowData, int rowIndex)
    {
        ImGui.TableNextRow((int)ImGuiTableRowFlags.None, 0.0f);

        // Row number column for row
        ImGui.TableSetColumnIndex(0);
        ImGui.SetNextItemWidth(10);
        ImGui.Text(rowIndex.ToString());

        for (int j = 0; j < SelectedTable.Columns.Count; j++)
        {
            ImGui.TableSetColumnIndex(1 + j);
            ImGui.SetNextItemWidth(-1);

            byte* valPtr = rowData + SelectedTable.Columns[j].Offset;
            switch (SelectedTable.Columns[j].Type)
            {
                case DBColumnType.SByte:
                    ImGui.InputScalar($"##cell_{rowIndex}_{j}", (int)ImGuiDataType.S8, (nint)valPtr, 0, 0, "%d", (int)ImGuiInputTextFlags.None);
                    break;
                case DBColumnType.Byte:
                    ImGui.InputScalar($"##cell_{rowIndex}_{j}", (int)ImGuiDataType.U8, (nint)valPtr, 0, 0, "%d", (int)ImGuiInputTextFlags.None);
                    break;
                case DBColumnType.Int:
                    ImGui.InputScalar($"##cell_{rowIndex}_{j}", (int)ImGuiDataType.S32, (nint)valPtr, 0, 0, "%d", (int)ImGuiInputTextFlags.None);
                    break;
                case DBColumnType.UInt:
                    ImGui.InputScalar($"##cell_{rowIndex}_{j}", (int)ImGuiDataType.U32, (nint)valPtr, 0, 0, "%X", (int)ImGuiInputTextFlags.None);
                    break;
                case DBColumnType.Short:
                    ImGui.InputScalar($"##cell_{rowIndex}_{j}", (int)ImGuiDataType.S16, (nint)valPtr, 0, 0, "%d", (int)ImGuiInputTextFlags.None);
                    break;
                case DBColumnType.Int64:
                    ImGui.InputScalar($"##cell_{rowIndex}_{j}", (int)ImGuiDataType.S64, (nint)valPtr, 0, 0, "%d", (int)ImGuiInputTextFlags.None);
                    break;
                case DBColumnType.HexUInt:
                    ImGui.InputScalar($"##cell_{rowIndex}_{j}", (int)ImGuiDataType.U32, (nint)valPtr, 0, 0, "%x", (int)ImGuiInputTextFlags.None);
                    break;

                case DBColumnType.Float:
                    ImGui.InputScalar($"##cell_{rowIndex}_{j}", (int)ImGuiDataType.Float, (nint)valPtr, 0, 0, "%f", (int)ImGuiInputTextFlags.None);
                    break;

                case DBColumnType.Double:
                    ImGui.InputScalar($"##cell_{rowIndex}_{j}", (int)ImGuiDataType.Double, (nint)valPtr, 0, 0, "%d", (int)ImGuiInputTextFlags.None);
                    break;

                case DBColumnType.HashString:

                    if (IdDatabase.Hashes.TryGetValue(*(uint*)valPtr, out string id))
                    {
                        nint strPtr = Marshal.StringToHGlobalAnsi(id);
                        ImGui.InputText($"##cell_{rowIndex}_{j}", (sbyte*)strPtr, id.Length + 1, (int)ImGuiInputTextFlags.None, null, 0);
                    }
                    else
                    {
                        nint strPtr = Marshal.StringToHGlobalAnsi((*(uint*)valPtr).ToString("X8"));
                        ImGui.InputText($"##cell_{rowIndex}_{j}", (sbyte*)strPtr, 9, (int)ImGuiInputTextFlags.None, null, 0);
                    }
                    break;

                case DBColumnType.RawString:
                    ImGui.InputText($"##cell_{rowIndex}_{j}", (sbyte*)valPtr, SelectedTable.Columns[j].StringLength, (int)ImGuiInputTextFlags.None, null, 0);
                    break;

                default:
                    break;

            }
        }
    }
}
