using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Text.Json;

namespace PlaidQuickstartBlazor.Shared.Tests.Unit;

[TestClass]
public class DataTableTest
{
    [TestMethod]
    public void Deserialize()
    {
        DataTable dataTable = SampleResult;

        var json = JsonSerializer.Serialize(dataTable);

        var actual = JsonSerializer.Deserialize<DataTable>(json);

        Assert.IsTrue(DataTablesEqual(dataTable,actual!));
    }

    private DataTable SampleResult => new DataTable()
    {
        Columns = (new[] { "A", "B", "C", "D", "E" })
        .Select(x => new Column() { Title = x })
        .ToArray(),

        Rows = new[]
        {
            new Row() { Cells = new[] { "1", "2", "3", "4", "5" } },
            new Row() { Cells = new[] { "1", "2", "3", "4", "5" } },
            new Row() { Cells = new[] { "1", "2", "3", "4", "5" } },
            new Row() { Cells = new[] { "1", "2", "3", "4", "5" } },
        }
    };

    private static bool DataTablesEqual(DataTable table1, DataTable table2)
    {
        if (table1.Rows.Length != table2.Rows.Length)
            return false;

        for (int i = 0; i < table1.Rows.Length; i++)
            if (!table1.Rows[i].Cells.SequenceEqual(table2.Rows[i].Cells))
                return false;

        if (table1.Columns.Length != table2.Columns.Length)
            return false;

        for (int i = 0; i < table1.Columns!.Length; i++)
        {
            if (table1.Columns[i].Title != table2.Columns[i].Title)
                return false;

            if (table1.Columns[i].IsRight != table2.Columns[i].IsRight)
                return false;
        }

        return true;
    }
}
