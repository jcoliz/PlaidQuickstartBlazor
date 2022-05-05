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

        Assert.AreEqual(dataTable, actual);
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

}