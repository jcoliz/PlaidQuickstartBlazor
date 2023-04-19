using Going.Plaid;
using Going.Plaid.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PlaidQuickstartBlazor.Shared;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text.Json;

namespace PlaidQuickstartBlazor.Server.Controllers;

/// <summary>
/// Retrieve data from Plaid service for logged-in user
/// </summary>
/// <remarks>
/// Handles all of the traffic from the Endpoint component
/// </remarks>
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

    [HttpGet]
    [ProducesResponseType(typeof(DataTable), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Shared.PlaidError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Auth()
    {
        var request = new Going.Plaid.Auth.AuthGetRequest();

        var response = await _client.AuthGetAsync(request);

        if (response.Error is not null)
            return Error(response.Error);

        Account? AccountFor(string? id) => response.Accounts.Where(x => x.AccountId == id).SingleOrDefault();

        DataTable result = new ServerDataTable("Name", "Balance/r", "Account #", "Routing #")
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
    [ProducesResponseType(typeof(Shared.PlaidError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Transactions()
    {
        // Set cursor to empty to receive all historical updates
        string cursor = String.Empty;
       
        // New transaction updates since "cursor"
        var added = new List<Transaction>();
        var modified = new List<Transaction>();
        var removed = new List<RemovedTransaction>();
        var hasMore = true;

        while (hasMore)
        {
            const int numrequested = 100;
            var request = new Going.Plaid.Transactions.TransactionsSyncRequest()
            {
                Cursor = cursor,
                Count = numrequested
            };

            var response = await _client.TransactionsSyncAsync(request);

            if (response.Error is not null)
                return Error(response.Error);

            added.AddRange(response!.Added);
            modified.AddRange(response.Modified);
            removed.AddRange(response.Removed);
            hasMore = response.HasMore;
            cursor = response.NextCursor;
        }

        const int numresults = 8;
        DataTable result = new ServerDataTable("Name", "Amount/r", "Date/r", "Category", "Channel")
        {
            Rows = added
                .OrderBy(x => x.Date)
                .TakeLast(numresults)
                .Select(x =>
                    new Row(
                        x.Name ?? string.Empty,
                        x.Amount?.ToString("C2") ?? string.Empty,
                        x.Date?.ToShortDateString() ?? string.Empty,
                        string.Join(':',x.Category ?? Enumerable.Empty<string>() ),
                        x.PaymentChannel.ToString() ?? string.Empty
                    )
                )
                .ToArray()
        };

        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(DataTable), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Shared.PlaidError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Identity()
    {
        var request = new Going.Plaid.Identity.IdentityGetRequest();

        var response = await _client.IdentityGetAsync(request);

        if (response.Error is not null)
            return Error(response.Error);

        DataTable result = new ServerDataTable("Names", "Emails", "Phone Numbers", "Addresses")
        {
            Rows = response.Accounts
                .SelectMany(a => 
                    a.Owners?
                        .Select(o => 
                            new Row(
                                string.Join(", ", o.Names),
                                string.Join(", ", o.Emails.Select(x => x.Data)),
                                string.Join(", ", o.PhoneNumbers.Select(x => x.Data)),
                                string.Join(", ", o.Addresses.Select(x => x.Data.Street))
                            )
                        )
                        ?? Enumerable.Empty<Row>()
                ).ToArray()
        };

        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(DataTable), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Shared.PlaidError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Holdings()
    {
        var request = new Going.Plaid.Investments.InvestmentsHoldingsGetRequest();

        var response = await _client.InvestmentsHoldingsGetAsync(request);

        if (response.Error is not null)
            return Error(response.Error);

        Security? SecurityFor(string? id) => response?.Securities.Where(x => x.SecurityId == id).SingleOrDefault();
        Account? AccountFor(string? id) => response?.Accounts.Where(x => x.AccountId == id).SingleOrDefault();

        DataTable result = new ServerDataTable("Mask", "Name", "Quantity/r", "Close Price/r", "Value/r")
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
    [ProducesResponseType(typeof(Shared.PlaidError), StatusCodes.Status400BadRequest)]
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

        DataTable result = new ServerDataTable("Name", "Amount/r", "Date/r", "Ticker")
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
    [ProducesResponseType(typeof(Shared.PlaidError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Balance()
    {
        var request = new Going.Plaid.Accounts.AccountsBalanceGetRequest();

        var response = await _client.AccountsBalanceGetAsync(request);

        if (response.Error is not null)
            return Error(response.Error);

        DataTable result = new ServerDataTable("Name", "AccountId", "Balance/r")
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
    [ProducesResponseType(typeof(Shared.PlaidError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Accounts()
    {
        var request = new Going.Plaid.Accounts.AccountsGetRequest();

        var response = await _client.AccountsGetAsync(request);

        if (response.Error is not null)
            return Error(response.Error);

        DataTable result = new ServerDataTable("Name", "Balance/r", "Subtype", "Mask")
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
    [ProducesResponseType(typeof(Shared.PlaidError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Item()
    {
        var request = new Going.Plaid.Item.ItemGetRequest();
        var response = await _client.ItemGetAsync(request);

        if (response.Error is not null)
            return Error(response.Error);

        _client.AccessToken = null;
        var intstrequest = new Going.Plaid.Institutions.InstitutionsGetByIdRequest() { InstitutionId = response.Item!.InstitutionId!, CountryCodes = new[] { CountryCode.Us } };
        var instresponse = await _client.InstitutionsGetByIdAsync(intstrequest);

        if (response.Error is not null)
            return Error(response.Error);

        DataTable result = new ServerDataTable("Institution Name", "Billed Products", "Available Products")
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
    [ProducesResponseType(typeof(Shared.PlaidError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Liabilities()
    {
        var request = new Going.Plaid.Liabilities.LiabilitiesGetRequest();

        var response = await _client.LiabilitiesGetAsync(request);

        if (response.Error is not null)
            return Error(response.Error);

        Account? AccountFor(string? id) => response.Accounts.Where(x => x.AccountId == id).SingleOrDefault();

        DataTable result = new ServerDataTable("Type", "Account", "Balance/r")
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

    [HttpGet]
    [ProducesResponseType(typeof(DataTable), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Shared.PlaidError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Payment()
    {
        var listrequest = new Going.Plaid.PaymentInitiation.PaymentInitiationPaymentListRequest();
        var listresponse = await _client.PaymentInitiationPaymentListAsync(listrequest);

        if (listresponse.Error is not null)
            return Error(listresponse.Error);

        var paymentid = listresponse.Payments.First().PaymentId;
        var request = new Going.Plaid.PaymentInitiation.PaymentInitiationPaymentGetRequest() { PaymentId = paymentid };
        var response = await _client.PaymentInitiationPaymentGetAsync(request);

        if (response.Error is not null)
            return Error(response.Error);

        DataTable result = new ServerDataTable("Payment ID", "Amount/r", "Status", "Status Update", "Recipient ID")
        {
            Rows = new Row[]
            {
                new Row(
                    paymentid,
                    response.Amount?.Value.ToString("C2") ?? string.Empty,
                    response.Status.ToString(),
                    response.LastStatusUpdate.ToString("MM-dd"),
                    response.RecipientId
                )
            }
        };

        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(DataTable), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Shared.PlaidError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Assets()
    {
        _client.AccessToken = null;
        var createrequest = new Going.Plaid.AssetReport.AssetReportCreateRequest()
        {
            AccessTokens = new[] { _credentials.AccessToken! },
            DaysRequested = 10,
            Options = new ()
            {
                ClientReportId = "Custom Report ID #123",
                User = new()
                {
                    ClientUserId = "Custom User ID #456",
                    FirstName = "Alice",
                    MiddleName = "Bobcat",
                    LastName = "Cranberry",
                    Ssn = "123-45-6789",
                    PhoneNumber = "555-123-4567",
                    Email = "alice@example.com"
                }
            }
        };
        var createresponse = await _client.AssetReportCreateAsync(createrequest);

        if (createresponse.Error is not null)
            return Error(createresponse.Error);

        var request = new Going.Plaid.AssetReport.AssetReportGetRequest() 
        { 
            AssetReportToken = createresponse.AssetReportToken            
        };

        var response = await _client.AssetReportGetAsync(request);
        int retries = 10;
        while (response?.Error?.ErrorCode == "PRODUCT_NOT_READY" && retries-- > 0)
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
            response = await _client.AssetReportGetAsync(request);
        }

        if (response?.Error is not null)
            return Error(response.Error);

        DataTable result = new ServerDataTable("Account", "Transactions/r", "Balance/r", "Days Available/r")
        {
            Rows = response!.Report.Items
                .SelectMany(x => x.Accounts.Select( a =>
                    new Row(
                        a.Name ?? string.Empty,
                        a.Transactions.Count.ToString(),
                        a.Balances?.Current?.ToString("C2") ?? string.Empty,
                        a.DaysAvailable.ToString("0")
                    ))
                )
                .ToArray()
        };

        // This would be the time to get the PDF report, however I don't see that Going.Plaid has that
        // ability.
        //
        // https://github.com/viceroypenguin/Going.Plaid/issues/63

        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(DataTable), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Shared.PlaidError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Transfer()
    {
        var actrequest = new Going.Plaid.Accounts.AccountsGetRequest();
        var actresponse = await _client.AccountsGetAsync(actrequest);

        if (actresponse.Error is not null)
            return Error(actresponse.Error);

        var accountid = actresponse.Accounts.FirstOrDefault()?.AccountId;
        var transrequest = new Going.Plaid.Transfer.TransferAuthorizationCreateRequest()
        {
            AccountId = accountid!,
            Amount = "1.34",
            Network = TransferNetwork.Ach,
            AchClass = AchClass.Ppd,
            Type = TransferType.Credit,
            User = new()
            {
                LegalName = "Alice Cranberry",
                PhoneNumber = "555-123-4567",
                EmailAddress = "alice@example.com"
            }
        };
        var transresponse = await _client.TransferAuthorizationCreateAsync(transrequest);

        if (transresponse.Error is not null)
            return Error(transresponse.Error);

        _logger.LogInformation($"Transfer Auth OK: {JsonSerializer.Serialize(transresponse)}");

        var authid = transresponse.Authorization.Id;

        var createrequest = new Going.Plaid.Transfer.TransferCreateRequest()
        {
            IdempotencyKey = "1223abc456xyz7890001",
            AccountId = accountid!,
            AuthorizationId = authid,
            Amount = "1.34",
            User = new()
            {
                LegalName = "Alice Cranberry",
                PhoneNumber = "555-123-4567",
                EmailAddress = "alice@example.com"
            }
        };
        var createresponse = await _client.TransferCreateAsync(createrequest);

        if (createresponse.Error is not null)
            return Error(createresponse.Error);

        _logger.LogInformation($"Transfer Create OK: {JsonSerializer.Serialize(createresponse)}");

        var transferid = createresponse.Transfer.Id;

        var request = new Going.Plaid.Transfer.TransferGetRequest()
        {
            TransferId = transferid,
        };
        var response = await _client.TransferGetAsync(request);

        if (response.Error is not null)
            return Error(response.Error);

        DataTable result = new ServerDataTable("Transfer ID", "Amount/r", "Type", "ACH Class", "Network", "Status")
        {
            Rows = new Row[]
            {
                new Row(
                    transferid,
                    response.Transfer.Amount,
                    response.Transfer.Type.ToString(),
                    response.Transfer.AchClass?.ToString() ?? string.Empty,
                    response.Transfer.AchClass?.ToString() ?? string.Empty,
                    response.Transfer.Status.ToString()
                )
            }
        };

        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(DataTable), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Shared.PlaidError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Verification()
    {
        var request = new Going.Plaid.Accounts.AccountsBalanceGetRequest();

        var response = await _client.AccountsBalanceGetAsync(request);

        if (response.Error is not null)
            return Error(response.Error);

        DataTable result = new ServerDataTable("Description", "Current Amount/r", "Currency")
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

    ObjectResult Error(Going.Plaid.Entity.PlaidError error, [CallerMemberName] string callerName = "")
    {
        var outerror = new ServerPlaidError(error);
        _logger.LogError($"{callerName}: {JsonSerializer.Serialize(outerror)}");

        return StatusCode(StatusCodes.Status400BadRequest, outerror);
    }

    /// <summary>
    /// Server-side version of shared data table
    /// </summary>
    /// <remarks>
    /// Contains code used only on server side. 
    /// Don't want to pollute client side with needless code.
    /// </remarks>
    internal class ServerDataTable: DataTable
    {
        internal ServerDataTable(params string[] cols)
        {
            Columns = cols.Select(x =>
            {
                var split = x.Split("/");
                return new Column() { Title = split[0], IsRight = split.Length > 1 && split[1] == "r" };
            }).ToArray();
        }
    }

    /// <summary>
    /// Server-side version of shared plaid error
    /// </summary>
    /// <remarks>
    /// TODO: Consider refactoring out this class
    /// Now that Going.Plaid 5.0.0 no longer uses enums for error type and error code,
    /// this class may not be needed anymore. May be able simply use the Going.Plaid
    /// error class directly when communicating with the client.
    /// </remarks>
    internal class ServerPlaidError: Shared.PlaidError
    {
        internal ServerPlaidError(Going.Plaid.Entity.PlaidError error)
        {
            try
            {
                base.error_message = error.ErrorMessage;
                base.display_message = error.DisplayMessage;

                base.error_type = error.ErrorType;
                base.error_code = error.ErrorCode;

                base.error_type_path = _error_type_paths.GetValueOrDefault(base.error_type);
            }
            catch
            {
                // If we run into errors here, we'll just take as much as we have converted sofar
            }
        }

        private readonly Dictionary<string, string> _error_type_paths = new Dictionary<string, string>()
        {
            { "ITEM_ERROR", "item" },
            { "INSTITUTION_ERROR", "institution" },
            { "API_ERROR", "api" },
            { "ASSET_REPORT_ERROR", "assets" },
            { "BANK_TRANSFER_ERROR", "bank-transfers" },
            { "INVALID_INPUT", "invalid-input" },
            { "INVALID_REQUEST", "invalid-request" },
            { "INVALID_RESULT", "invalid-result" },
            { "OAUTH_ERROR", "oauth" },
            { "PAYMENT_ERROR", "payment" },
            { "RATE_LIMIT_EXCEEDED", "rate-limit-exceeded" },
            { "RECAPTCHA_ERROR", "recaptcha" },
            { "SANDBOX_ERROR", "sandbox" },
        };
    }
}
