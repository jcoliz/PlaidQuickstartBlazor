module ProductSteps

open NUnit.Framework
open System
open TickSpec
open Microsoft.Playwright
open FsUnit

//
// GIVEN
//

let [<Given>] ``user launched site`` (page: IPage) (uri: Uri) = 
    page.GotoAsync(uri.ToString()) |> Async.AwaitTask |> Async.RunSynchronously

let [<Given>] ``link flow complete`` (page: IPage) =
    page.ClickAsync("data-test-id=launch-link") |> Async.AwaitTask |> Async.RunSynchronously
    page.WaitForLoadStateAsync( LoadState.DOMContentLoaded ) |> Async.AwaitTask |> Async.RunSynchronously

// 
// WHEN
//

let [<When>] ``clicking send request for the (.*) product`` (product:string) (page: IPage) =
    page.ClickAsync($"data-test-id=endpoint-{product} >> data-test-id=btn-request") |> Async.AwaitTask |> Async.RunSynchronously

//
// THEN
//

let [<Then>] ``a table of results is populated`` (page: IPage) =
    page.WaitForSelectorAsync("data-test-id=Table")  |> Async.AwaitTask |> Async.RunSynchronously

let [<Then>] ``the table has (.*) columns and (.*) rows`` (columns:int) (rows:int) (page: IPage) =
    page.Locator("data-test-id=Table >> thead >> th").CountAsync() 
        |> Async.AwaitTask 
        |> Async.RunSynchronously 
        |> should equal columns
    
    if (rows > 0) then
        page.Locator("data-test-id=Table >> tbody >> tr").CountAsync() 
            |> Async.AwaitTask 
            |> Async.RunSynchronously 
            |> should equal rows
    
let [<Then>] ``save a screenshot named "(.*)"`` (name:string) (page:IPage) =
    let filename = $"Screenshot/{name}.png";
    let options = new PageScreenshotOptions ( Path = filename, FullPage = true, OmitBackground = true )
    page.ScreenshotAsync(options) |> Async.AwaitTask |> Async.RunSynchronously |> ignore
    TestContext.AddTestAttachment(filename)
