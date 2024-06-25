using GBFRDataTools.Database.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gbfr.utility.modtools;

public unsafe class DatabaseTable
{
    public string Name { get; set; }
    public List<TableColumn> Columns { get; set; }
    public int RowSize { get; set; }
    public StdUnorderedMap* Rows { get; set; }

    public DatabaseTable(string name, List<TableColumn> columns, int rowSize, StdUnorderedMap* rows)
    {
        Name = name;
        Columns = columns;
        RowSize = rowSize;
        Rows = rows;
    }
}
