using Going.Plaid;
using Going.Plaid.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PlaidQuickstartBlazor.Shared;
using System.Runtime.CompilerServices;
using System.Text.Json;

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

    private DataTable SampleResult => new()
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
    [ProducesResponseType(typeof(DataTable), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Auth()
    {
        var request = new Going.Plaid.Auth.AuthGetRequest();

        var response = await _client.AuthGetAsync(request);

        if (response.Error is not null)
            return Error(response.Error);

        Account? AccountFor(string? id) => response.Accounts.Where(x => x.AccountId == id).SingleOrDefault();

        var result = new DataTable("Name", "Balance/r", "Account #", "Routing #")
        {
            Rows = response.Numbers.Ach
                .Select(x =>
                    new Row(
                        AccountFor(x.AccountId)?.Name ?? String.Empty,
                        AccountFor(x.AccountId)?.Balances?.Current?.ToString("C2") ?? string.Empty,
                        x.Account,
                        x.Routing
                    )
                )
                .ToArray()
        };

        return Ok(result);
    }
    [HttpGet]
    [ProducesResponseType(typeof(DataTable), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Transactions()
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

        if (response.Error is not null)
            return Error(response.Error);

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

        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(DataTable), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Identity()
    {
        var request = new Going.Plaid.Identity.IdentityGetRequest();

        var response = await _client.IdentityGetAsync(request);

        if (response.Error is not null)
            return Error(response.Error);

        var result = new DataTable("Names", "Emails", "Phone Numbers", "Addresses")
        {
            Rows = response.Accounts
                .SelectMany(a => 
                    a.Owners
                        .Select(o => 
                            new Row(
                                string.Join(", ", o.Names),
                                string.Join(", ", o.Emails.Select(x => x.Data)),
                                string.Join(", ", o.PhoneNumbers.Select(x => x.Data)),
                                string.Join(", ", o.Addresses.Select(x => x.Data.Street))
                            )
                        )
                ).ToArray()
        };

        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(DataTable), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Holdings()
    {
        var request = new Going.Plaid.Investments.InvestmentsHoldingsGetRequest();

        var response = await _client.InvestmentsHoldingsGetAsync(request);

        if (response.Error is not null)
            return Error(response.Error);

        Security? SecurityFor(string? id) => response?.Securities.Where(x => x.SecurityId == id).SingleOrDefault();
        Account? AccountFor(string? id) => response?.Accounts.Where(x => x.AccountId == id).SingleOrDefault();

        var result = new DataTable("Mask", "Name", "Quantity/r", "Close Price/r", "Value/r")
        {
            Rows = response.Holdings
            .Select(x =>
                new Row(
                    AccountFor(x.AccountId)?.Mask ?? string.Empty,
                    SecurityFor(x.SecurityId)?.Name ?? string.Empty,
                    x.Quantity.ToString("0.000"),
                    x.InstitutionPrice.ToString("C2"),
                    x.InstitutionValue.ToString("C2")
                )
            )
            .ToArray()
        };

        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(DataTable), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Investments_Transactions()
    {
        var request = new Going.Plaid.Investments.InvestmentsTransactionsGetRequest()
        {
            Options = new InvestmentsTransactionsGetRequestOptions()
            {
                Count = 100
            },
            StartDate = DateOnly.FromDateTime( DateTime.Now - TimeSpan.FromDays(30) ),
            EndDate = DateOnly.FromDateTime(DateTime.Now)
        };

        var response = await _client.InvestmentsTransactionsGetAsync(request);

        if (response.Error is not null)
            return Error(response.Error);

        Security? SecurityFor(string? id) => response?.Securities.Where(x => x.SecurityId == id).SingleOrDefault();

        var result = new DataTable("Name", "Amount/r", "Date/r", "Ticker")
        {
            Rows = response.InvestmentTransactions
            .Select(x =>
                new Row(
                    x.Name,
                    x.Amount.ToString("C2"),
                    x.Date.ToShortDateString(),
                    SecurityFor(x.SecurityId)?.TickerSymbol ?? string.Empty
                )
            )
            .ToArray()
        };

        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(DataTable), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Balance()
    {
        var request = new Going.Plaid.Accounts.AccountsBalanceGetRequest();

        var response = await _client.AccountsBalanceGetAsync(request);

        if (response.Error is not null)
            return Error(response.Error);

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

        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(DataTable), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Accounts()
    {
        var request = new Going.Plaid.Accounts.AccountsGetRequest();

        var response = await _client.AccountsGetAsync(request);

        if (response.Error is not null)
            return Error(response.Error);

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

        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(DataTable), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Item()
    {
        var request = new Going.Plaid.Item.ItemGetRequest();
        var response = await _client.ItemGetAsync(request);

        if (response.Error is not null)
            return StatusCode((int)response.StatusCode, response.Error.ErrorMessage);

        _client.AccessToken = null;
        var intstrequest = new Going.Plaid.Institutions.InstitutionsGetByIdRequest() { InstitutionId = response.Item!.InstitutionId!, CountryCodes = new[] { CountryCode.Us } };
        var instresponse = await _client.InstitutionsGetByIdAsync(intstrequest);

        if (response.Error is not null)
            return Error(response.Error);

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
    [ProducesResponseType(typeof(DataTable), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Liabilities()
    {
        var request = new Going.Plaid.Liabilities.LiabilitiesGetRequest();

        var response = await _client.LiabilitiesGetAsync(request);

        if (response.Error is not null)
            return Error(response.Error);

        Account? AccountFor(string? id) => response.Accounts.Where(x => x.AccountId == id).SingleOrDefault();

        var result = new DataTable("Type", "Account", "Balance/r")
        {
            Rows = response.Liabilities!.Credit!
                .Select(x =>
                    new Row(
                        "Credit",
                        AccountFor(x.AccountId)?.Name ?? string.Empty,
                        x.LastStatementBalance?.ToString("C2") ?? string.Empty
                    )
                )
                .Concat(
                    response.Liabilities!.Student!
                        .Select(x=>
                            new Row(
                                "Student Loan",
                                AccountFor(x.AccountId)?.Name ?? string.Empty,
                                AccountFor(x.AccountId)?.Balances?.Current?.ToString("C2") ?? string.Empty
                            )
                        )
                )
                .Concat(
                    response.Liabilities!.Mortgage!
                        .Select(x =>
                            new Row(
                                "Mortgage",
                                AccountFor(x.AccountId)?.Name ?? string.Empty,
                                AccountFor(x.AccountId)?.Balances?.Current?.ToString("C2") ?? string.Empty
                            )
                        )
                )
                .ToArray()
        };

        return Ok(result);
    }

    ObjectResult Error(Going.Plaid.Errors.PlaidError error, [CallerMemberName] string callerName = "")
    {
        _logger.LogError($"{callerName}: {JsonSerializer.Serialize(error)}");
        return StatusCode(StatusCodes.Status400BadRequest, error);
    }
}
