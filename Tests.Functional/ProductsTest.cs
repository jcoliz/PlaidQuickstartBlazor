using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Playwright;
using System;
using Tests.Functional.Base;

namespace StarterKit.Tests.Functional;

/// <summary>
/// Test each of the products
/// </summary>
[TestClass]
public class ProductsTest: FunctionalTest
{
    #region Tests

    /// <summary>
    /// [User Can] Access the product {name}
    /// </summary>
    [DataRow("auth", 4, 2)]
    [DataRow("transactions", 5, null)]
    [DataRow("identity", 4, 9)]
    [DataRow("assets", 4, 9)]
    [DataRow("balance", 3, 9)]
    [DataRow("investments", 5, 12)]
    [DataRow("investmentstransactions", 4, null)]
    [DataRow("liabilities", 3, 3)]
    [DataRow("itemget", 3, 1)]
    [DataRow("accountsget", 4, 9)]
    [DataTestMethod]
    public async Task Product(string name, int expected_cols_count, int? expected_rows_count)
    {
        // Given: On the root of the site
        await Page!.GotoAsync(TestContext?.Properties?["webAppUrl"] as string ?? throw new ApplicationException());

        // And: Link flow complete 
        await Page!.ClickAsync("data-test-id=launch-link");
        await Page!.WaitForLoadStateAsync( LoadState.DOMContentLoaded );
        await SaveScreenshotAsync($"{name}-0-Ready");

        // When: Clicking on "Send Request" within the "{name}}" product
        await Page!.ClickAsync($"data-test-id=endpoint-{name} >> data-test-id=btn-request");

        // Then: A table of results is populated
        await Page!.WaitForSelectorAsync("data-test-id=Table");
        await SaveScreenshotAsync($"{name}-1-Loaded");

        // And: The shape of the table matches our expectations
        var actual_cols = Page!.Locator("data-test-id=Table >> thead >> th");
        var actual_cols_count = await actual_cols.CountAsync();
        Assert.AreEqual(expected_cols_count,actual_cols_count);

        if (expected_rows_count.HasValue)
        {
            var actual_rows = Page!.Locator("data-test-id=Table >> tbody >> tr");
            var actual_rows_count = await actual_rows.CountAsync();
            Assert.AreEqual(expected_rows_count.Value,actual_rows_count);
        }
    }

    #endregion
}