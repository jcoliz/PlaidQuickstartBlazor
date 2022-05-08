using System.Threading.Tasks;
using System.Linq;
using Microsoft.Playwright.MSTest;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Playwright;
using System;

namespace Tests.Functional.Base;

/// <summary>
/// Base test class shared by all functional test classes
/// </summary>
public class FunctionalTest: PageTest
{
    #region Overrides

    public override BrowserNewContextOptions ContextOptions => 
        new BrowserNewContextOptions
        {
            AcceptDownloads = true,
            ViewportSize = new ViewportSize() { Width = 1080, Height = 810 }
        };

    #endregion

    #region Helpers

    protected async Task SaveScreenshotAsync(string? moment = null)
    {
        var testname = $"{TestContext!.FullyQualifiedTestClassName.Split(".").Last()}/{TestContext.TestName}";

        var displaymoment = string.IsNullOrEmpty(moment) ? string.Empty : $"-{moment.Replace('/','-')}";

        var filename = $"Screenshot/{testname}{displaymoment}.png";
        await Page!.ScreenshotAsync(new PageScreenshotOptions() { Path = filename, OmitBackground = true, FullPage = true });
        TestContext.AddResultFile(filename);
    }

    #endregion

}