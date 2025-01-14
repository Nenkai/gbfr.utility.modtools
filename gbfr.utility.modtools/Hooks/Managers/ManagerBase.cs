using CppSharp.Runtime;
using GBFRDataTools.Database.Entities;
using GBFRDataTools.Database;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
namespace gbfr.utility.modtools.Hooks.Managers;

public class TableManagerBase
{
    public List<DatabaseTable> Tables { get; set; } = [];

    public unsafe void AddTableMap(string name, StdUnorderedMap* rows, bool isVectorMap = false)
    {
        List<TableColumn> columns = TableMappingReader.ReadColumnMappings(name, new Version(1, 3, 1), out int readSize);

        var table = new DatabaseTable(name, columns, readSize, rows, isVectorMap);
        Tables.Add(table);
    }

    public unsafe void AddTableVector(string name, StdVector* rows)
    {
        List<TableColumn> columns = TableMappingReader.ReadColumnMappings(name, new Version(1, 3, 1), out int readSize);
        var table = new DatabaseTable(name, columns, readSize, rows);

        Tables.Add(table);
    }
}
