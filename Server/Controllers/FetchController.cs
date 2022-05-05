using Going.Plaid;
using Going.Plaid.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PlaidQuickstartBlazor.Server.Helpers;
using PlaidQuickstartBlazor.Shared;

namespace PlaidQuickstartBlazor.Server.Controllers;

[ApiController]
[Route("[controller]/[action]")]
[Produces("application/json")]
public class FetchController : ControllerBase
{
    private readonly ILogger<FetchController> _logger;
    private readonly PlaidCredentials _credentials;
    private readonly PlaidClient _client;

    public FetchController(ILogger<FetchController> logger, IOptions<PlaidCredentials> credentials, PlaidClient client)
    {
        _logger = logger;
        _credentials = credentials.Value;
        _client = client;
        _client.AccessToken = _credentials.AccessToken;
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

    [HttpGet]
    public DataTable Auth()
    {
        _logger.LogInformation("Auth");

        return SampleResult;
    }
    [HttpGet]
    public async Task<DataTable> Transactions()
    {
        var request = new Going.Plaid.Transactions.TransactionsGetRequest()
        {
            Options = new TransactionsGetRequestOptions()
            {
                Count = 100
            },
            StartDate = DateOnly.FromDateTime( DateTime.Now - TimeSpan.FromDays(30) ),
            EndDate = DateOnly.FromDateTime(DateTime.Now)
        };

        var response = await _client.TransactionsGetAsync(request);

        var result = new DataTable("Name", "Amount/r", "Date/r", "Category", "Channel")
        {
            Rows = response.Transactions
            .Select(x =>
                new Row(
                    x.Name,
                    x.Amount.ToString("C2"),
                    x.Date.ToShortDateString(),
                    string.Join(':',x.Category ?? Enumerable.Empty<string>() ),
                    x.PaymentChannel.ToString()
                )
            )
            .ToArray()
        };

        return result;
    }
    [HttpGet]
    public DataTable Identity()
    {
        _logger.LogInformation("Identity");

        return SampleResult;
    }
    [HttpGet]
    public DataTable Holdings()
    {
        _logger.LogInformation("Holdings");

        return SampleResult;
    }
    [HttpGet]
    public DataTable Investments_Transactions()
    {
        return SampleResult;
    }
    [HttpGet]
    public async Task<DataTable> Balance()
    {
        var request = new Going.Plaid.Accounts.AccountsBalanceGetRequest();

        var response = await _client.AccountsBalanceGetAsync(request);

        var result = new DataTable("Name", "AccountId", "Balance/r")
        {
            Rows = response.Accounts
                .Select(x =>
                    new Row(
                        x.Name,
                        x.AccountId,
                        x.Balances?.Current?.ToString("C2") ?? string.Empty
                    )
                )
                .ToArray()
        };

        return result;
    }

    [HttpGet]
    public async Task<DataTable> Accounts()
    {
        var request = new Going.Plaid.Accounts.AccountsGetRequest();

        var response = await _client.AccountsGetAsync(request);

        var result = new DataTable("Name", "Balance/r", "Subtype", "Mask")
        {
            Rows = response.Accounts
                .Select(x =>
                    new Row(
                        x.Name,
                        x.Balances?.Current?.ToString("C2") ?? string.Empty,
                        x.Subtype?.ToString() ?? string.Empty,
                        x.Mask ?? string.Empty
                    )
                )
                .ToArray()
        };

        return result;
    }

    [HttpGet]
    public async Task<ActionResult> Item()
    {
        var request = new Going.Plaid.Item.ItemGetRequest();
        var response = await _client.ItemGetAsync(request);

        if (response.Error is not null)
            return StatusCode((int)response.StatusCode, response.Error.ErrorMessage);

        _client.AccessToken = null;
        var intstrequest = new Going.Plaid.Institutions.InstitutionsGetByIdRequest() { InstitutionId = response.Item!.InstitutionId!, CountryCodes = new[] { CountryCode.Us } };
        var instresponse = await _client.InstitutionsGetByIdAsync(intstrequest);

        if (instresponse.Error is not null)
            return StatusCode((int)instresponse.StatusCode, instresponse.Error.ErrorMessage);

        var result = new DataTable("Institution Name", "Billed Products", "Available Products")
        {
            Rows = new[] 
            {
                new Row(
                    instresponse.Institution.Name,
                    string.Join(",",response.Item.BilledProducts.Select(x=>x.ToString())),
                    string.Join(",",response.Item.AvailableProducts.Select(x=>x.ToString()))
                )
            }
        };

        return Ok(result);
    }

    [HttpGet]
    public async Task<DataTable> Liabilities()
    {

        var request = new Going.Plaid.Liabilities.LiabilitiesGetRequest();

        var response = await _client.LiabilitiesGetAsync(request);

        Account? AccountFor(string? id) => response.Accounts.Where(x => x.AccountId == id).SingleOrDefault();

        string AccountNameFor(string id) => AccountFor(id)?.Name ?? string.Empty;

        var result = new DataTable("Type", "Account", "Balance/r")
        {
            Rows = response.Liabilities!.Credit!
                .Select(x =>
                    new Row(
                        "Credit",
                        AccountNameFor(x.AccountId ?? string.Empty),
                        x.LastStatementBalance?.ToString("C2") ?? string.Empty
                    )
                )
                .Concat(
                    response.Liabilities!.Student!
                        .Select(x=>
                            new Row(
                                "Student Loan",
                                AccountNameFor(x.AccountId ?? string.Empty),
                                AccountFor(x.AccountId)?.Balances?.Current?.ToString("C2") ?? string.Empty
                            )
                        )
                )
                .Concat(
                    response.Liabilities!.Mortgage!
                        .Select(x =>
                            new Row(
                                "Mortgage",
                                AccountNameFor(x.AccountId ?? string.Empty),
                                AccountFor(x.AccountId)?.Balances?.Current?.ToString("C2") ?? string.Empty
                            )
                        )
                )
                .ToArray()
        };

        return result;
    }
}
