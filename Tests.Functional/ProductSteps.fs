module ProductSteps

open NUnit.Framework
open System
open TickSpec
open Microsoft.Playwright
open FsUnit
open System.Threading.Tasks

let Await x =
    x |> Async.AwaitTask |> Async.RunSynchronously

//
// GIVEN
//

let [<Given>] ``user launched site`` (page: IPage) (uri: Uri) = 
    page.GotoAsync(uri.ToString()) |> Await

let [<Given>] ``link flow complete`` (page: IPage) =
    page.ClickAsync("data-test-id=launch-link") |> Async.AwaitTask |> Async.RunSynchronously
    page.WaitForLoadStateAsync( LoadState.DOMContentLoaded ) |> Async.AwaitTask |> Async.RunSynchronously

// 
// WHEN
//

let [<When>] ``clicking (.*) in the (.*) endpoint`` (element: string) (product: string) (page: IPage) =
    page.ClickAsync($"data-test-id=endpoint-{product} >> data-test-id={element}") |> Async.AwaitTask |> Async.RunSynchronously

//
// THEN
//

let [<Then>] ``a (.*) is returned`` (testid: string) (page: IPage) =
    let locator = page.Locator($"data-test-id={testid}")
    locator.WaitForAsync() |> Async.AwaitTask |> Async.RunSynchronously
    locator.CountAsync() 
        |> Await
        |> should equal 1
    locator

let [<Then>] ``it has (.*) columns and (.*) rows`` (columns: int) (rows: int) (table: ILocator) =
    table.Locator("thead >> th").CountAsync() 
        |> Await 
        |> should equal columns
    
    if (rows > 0) then
        table.Locator("tbody >> tr").CountAsync() 
            |> Await 
            |> should equal rows
    
let [<Then>] ``save a screenshot named "(.*)"`` (name: string) (page: IPage) =
    let filename = $"Screenshot/{name}.png";
    let options = new PageScreenshotOptions ( Path = filename, FullPage = true, OmitBackground = true )
    page.ScreenshotAsync(options) 
        |> Await
        |> Array.length 
        |> should be (greaterThan 10000)
    TestContext.AddTestAttachment(filename)
