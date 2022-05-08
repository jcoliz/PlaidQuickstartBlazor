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
    /// [User Can] Access the Auth product
    /// </summary>
    [TestMethod]
    public async Task Auth()
    {
        // Given: On the root of the site
        await Page!.GotoAsync(TestContext?.Properties?["webAppUrl"] as string ?? throw new ApplicationException());

        // And: Link flow complete 
        await Page!.WaitForSelectorAsync("data-test-id=launch-link");
        await Page!.ClickAsync("data-test-id=launch-link");
        await Page!.WaitForLoadStateAsync( LoadState.DOMContentLoaded );
        await SaveScreenshotAsync("0-Ready");

        // When: Clicking on "Send Request" within the "Auth" product
        await Page!.ClickAsync("data-test-id=request-Auth");

        // Then: A table of results is populated
        await Page!.WaitForSelectorAsync("data-test-id=Table");
        await SaveScreenshotAsync("1-Loaded");

        // And: The shape of the table matches our expectations
        var ths = Page!.Locator("data-test-id=Table >> thead >> th");
        var ths_count = await ths.CountAsync();
        Assert.AreEqual(4,ths_count);

        var trs = Page!.Locator("data-test-id=Table >> tbody >> tr");
        var trs_count = await trs.CountAsync();
        Assert.AreEqual(2,trs_count);
    }

    #endregion
}