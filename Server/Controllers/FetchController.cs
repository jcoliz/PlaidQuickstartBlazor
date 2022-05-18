using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PlaidApi;
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
    }

    [HttpGet]
    [ProducesResponseType(typeof(DataTable), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PlaidWireError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Auth()
    {
        try
        {
            var request = new AuthGetRequest() { Access_token = _credentials.AccessToken! };

            // FAILS on .NET json: {"The JSON value could not be converted to System.Nullable`1[AccountSubtype]. Path: $.accounts[3].subtype | LineNumber: 59 | BytePositionInLine: 30."}
            // Maybe this? https://github.com/Macross-Software/core/tree/develop/ClassLibraries/Macross.Json.Extensions

            var response = await _client.AuthGetAsync(request);

            AccountBase? AccountFor(string? id) => response!.Accounts.Where(x => x.Account_id == id).SingleOrDefault();

            DataTable result = new ServerDataTable("Name", "Balance/r", "Account #", "Routing #")
            {
                Rows = response.Numbers.Ach
                    .Select(x =>
                        new Row(
                            AccountFor(x.Account_id)?.Name ?? String.Empty,
                            AccountFor(x.Account_id)?.Balances?.Current?.ToString("C2") ?? string.Empty,
                            x.Account,
                            x.Routing
                        )
                    )
                    .ToArray()
            };

            return Ok(result);
        }
        catch (ApiException<Error> ex)
        {
            return Error(ex.Result);
        }
        catch (Exception ex)
        {
            return Error(ex);
        }
    }
    [HttpGet]
    [ProducesResponseType(typeof(DataTable), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PlaidError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Transactions()
    {
        try
        {
            var request = new TransactionsGetRequest()
            {
                Access_token = _credentials.AccessToken!,
                Options = new TransactionsGetRequestOptions()
                {
                    Count = 100
                },
                Start_date = DateTime.Now - TimeSpan.FromDays(30),
                End_date = DateTime.Now
            };
            var response = await _client.TransactionsGetAsync(request);

            DataTable result = new ServerDataTable("Name", "Amount/r", "Date/r", "Category", "Channel")
            {
                Rows = response.Transactions
                    .Select(x =>
                        new Row(
                            x.Name,
                            x.Amount.ToString("C2"),
                            x.Date.ToString("MM-dd"),
                            string.Join(':', x.Category ?? Enumerable.Empty<string>()),
                            x.Payment_channel.ToString()
                        )
                    )
                    .ToArray()
            };

            return Ok(result);

        }
        catch (ApiException<Error> ex)
        {
            return Error(ex.Result);
        }
        catch (Exception ex)
        {
            return Error(ex);
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(DataTable), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PlaidError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Identity()
    {
        try
        {
            var request = new IdentityGetRequest() { Access_token = _credentials.AccessToken! };

            var response = await _client.IdentityGetAsync(request);

            DataTable result = new ServerDataTable("Names", "Emails", "Phone Numbers", "Addresses")
            {
                Rows = response.Accounts
                    .SelectMany(a =>
                        a.Owners
                            .Select(o =>
                                new Row(
                                    string.Join(", ", o.Names),
                                    string.Join(", ", o.Emails.Select(x => x.Data)),
                                    string.Join(", ", o.Phone_numbers.Select(x => x.Data)),
                                    string.Join(", ", o.Addresses.Select(x => x.Data.Street))
                                )
                            )
                    ).ToArray()
            };

            return Ok(result);
        }
        catch (ApiException<Error> ex)
        {
            return Error(ex.Result);
        }
        catch (Exception ex)
        {
            return Error(ex);
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(DataTable), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PlaidError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Holdings()
    {
        try
        {
            var request = new InvestmentsHoldingsGetRequest() { Access_token = _credentials.AccessToken! };

            var response = await _client.InvestmentsHoldingsGetAsync(request);

            Security? SecurityFor(string? id) => response?.Securities.Where(x => x.Security_id == id).SingleOrDefault();
            AccountBase? AccountFor(string? id) => response?.Accounts.Where(x => x.Account_id == id).SingleOrDefault();

            DataTable result = new ServerDataTable("Mask", "Name", "Quantity/r", "Close Price/r", "Value/r")
            {
                Rows = response.Holdings
                .Select(x =>
                    new Row(
                        AccountFor(x.Account_id)?.Mask ?? string.Empty,
                        SecurityFor(x.Security_id)?.Name ?? string.Empty,
                        x.Quantity.ToString("0.000"),
                        x.Institution_price.ToString("C2"),
                        x.Institution_price.ToString("C2")
                    )
                )
                .ToArray()
            };


            return Ok(result);
        }
        catch (ApiException<Error> ex)
        {
            return Error(ex.Result);
        }
        catch (Exception ex)
        {
            return Error(ex);
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(DataTable), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PlaidError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Investments_Transactions()
    {
        try
        {
            var request = new InvestmentsTransactionsGetRequest()
            {
                Access_token = _credentials.AccessToken!,
                Options = new InvestmentsTransactionsGetRequestOptions()
                {
                    Count = 100
                },
                Start_date = DateTime.Now - TimeSpan.FromDays(30),
                End_date = DateTime.Now
            };

            var response = await _client.InvestmentsTransactionsGetAsync(request);

            Security? SecurityFor(string? id) => response?.Securities.Where(x => x.Security_id == id).SingleOrDefault();

            DataTable result = new ServerDataTable("Name", "Amount/r", "Date/r", "Ticker")
            {
                Rows = response.Investment_transactions
                .Select(x =>
                    new Row(
                        x.Name,
                        x.Amount.ToString("C2"),
                        x.Date.ToString("MM-dd"),
                        SecurityFor(x.Security_id)?.Ticker_symbol ?? string.Empty
                    )
                )
                .ToArray()
            };

            return Ok(result);
        }
        catch (ApiException<Error> ex)
        {
            return Error(ex.Result);
        }
        catch (Exception ex)
        {
            return Error(ex);
        }

    }

    [HttpGet]
    [ProducesResponseType(typeof(DataTable), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PlaidError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Balance()
    {
        try
        {
            var request = new AccountsBalanceGetRequest() { Access_token = _credentials.AccessToken! };

            var response = await _client.AccountsBalanceGetAsync(request);

            DataTable result = new ServerDataTable("Name", "AccountId", "Balance/r")
            {
                Rows = response.Accounts
                    .Select(x =>
                        new Row(
                            x.Name,
                            x.Account_id,
                            x.Balances?.Current?.ToString("C2") ?? string.Empty
                        )
                    )
                    .ToArray()
            };
            return Ok(result);
        }
        catch (ApiException<Error> ex)
        {
            return Error(ex.Result);
        }
        catch (Exception ex)
        {
            return Error(ex);
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(DataTable), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PlaidError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Accounts()
    {
        try
        {
            var request = new AccountsGetRequest() { Access_token = _credentials.AccessToken! };

            var response = await _client.AccountsGetAsync(request);

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
        catch (ApiException<Error> ex)
        {
            return Error(ex.Result);
        }
        catch (Exception ex)
        {
            return Error(ex);
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(DataTable), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PlaidError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Item()
    {
        try
        {
            var request = new ItemGetRequest() { Access_token = _credentials.AccessToken! };
            var response = await _client.ItemGetAsync(request);

            var intstrequest = new InstitutionsGetByIdRequest() { Institution_id = response.Item!.Institution_id!, Country_codes= new[] { CountryCode.US } };
            var instresponse = await _client.InstitutionsGetByIdAsync(intstrequest);

            DataTable result = new ServerDataTable("Institution Name", "Billed Products", "Available Products")
            {
                Rows = new[]
                {
                    new Row(
                        instresponse.Institution.Name,
                        string.Join(",",response.Item.Billed_products.Select(x=>x.ToString())),
                        string.Join(",",response.Item.Available_products.Select(x=>x.ToString()))
                    )
                }
            };

            return Ok(result);
        }
        catch (ApiException<Error> ex)
        {
            return Error(ex.Result);
        }
        catch (Exception ex)
        {
            return Error(ex);
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(DataTable), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PlaidError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Liabilities()
    {
        try
        {
            var request = new LiabilitiesGetRequest() { Access_token = _credentials.AccessToken! };

            var response = await _client.LiabilitiesGetAsync(request);

            AccountBase? AccountFor(string? id) => response.Accounts.Where(x => x.Account_id == id).SingleOrDefault();

            DataTable result = new ServerDataTable("Type", "Account", "Balance/r")
            {
                Rows = response.Liabilities!.Credit!
                    .Select(x =>
                        new Row(
                            "Credit",
                            AccountFor(x.Account_id)?.Name ?? string.Empty,
                            x.Last_statement_balance?.ToString("C2") ?? string.Empty
                        )
                    )
                    .Concat(
                        response.Liabilities!.Student!
                            .Select(x =>
                                new Row(
                                    "Student Loan",
                                    AccountFor(x.Account_id)?.Name ?? string.Empty,
                                    AccountFor(x.Account_id)?.Balances?.Current?.ToString("C2") ?? string.Empty
                                )
                            )
                    )
                    .Concat(
                        response.Liabilities!.Mortgage!
                            .Select(x =>
                                new Row(
                                    "Mortgage",
                                    AccountFor(x.Account_id)?.Name ?? string.Empty,
                                    AccountFor(x.Account_id)?.Balances?.Current?.ToString("C2") ?? string.Empty
                                )
                            )
                    )
                    .ToArray()
            };

            return Ok(result);
        }
        catch (ApiException<Error> ex)
        {
            return Error(ex.Result);
        }
        catch (Exception ex)
        {
            return Error(ex);
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(DataTable), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PlaidError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Payment()
    {
        try
        {
            var listrequest = new PaymentInitiationPaymentListRequest();
            var listresponse = await _client.PaymentInitiationPaymentListAsync(listrequest);

            var paymentid = listresponse.Payments.First().Payment_id;
            var request = new PaymentInitiationPaymentGetRequest() 
            { 
                Payment_id = paymentid 
            };
            var response = await _client.PaymentInitiationPaymentGetAsync(request);

            DataTable result = new ServerDataTable("Payment ID", "Amount/r", "Status", "Status Update", "Recipient ID")
            {
                Rows = new Row[]
                {
                new Row(
                    paymentid,
                    response.Amount?.Value.ToString("C2") ?? string.Empty,
                    response.Status.ToString(),
                    response.Last_status_update.ToString("MM-dd"),
                    response.Recipient_id
                )
                }
            };

            return Ok(result);
        }
        catch (ApiException<Error> ex)
        {
            return Error(ex.Result);
        }
        catch (Exception ex)
        {
            return Error(ex);
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(DataTable), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PlaidError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Assets()
    {
        try
        {
            var createrequest = new AssetReportCreateRequest()
            {
                Access_tokens = new[] { _credentials.AccessToken! },
                Days_requested = 10,
                Options = new()
                {
                    Client_report_id = "Custom Report ID #123",
                    User = new()
                    {
                        Client_user_id = "Custom User ID #456",
                        First_name = "Alice",
                        Middle_name = "Bobcat",
                        Last_name = "Cranberry",
                        Ssn = "123-45-6789",
                        Phone_number = "555-123-4567",
                        Email = "alice@example.com"
                    }
                }
            };
            var createresponse = await _client.AssetReportCreateAsync(createrequest);

            var request = new AssetReportGetRequest()
            {
                Asset_report_token = createresponse.Asset_report_token
            };

            _logger.LogInformation($"Assets: Token {createresponse.Asset_report_token}");

            AssetReportGetResponse? response = null;
            int retries = 10;
            while (retries-- > 0)
            {
                try
                {
                    response = await _client.AssetReportGetAsync(request);
                }
                catch (ApiException ex)
                {
                    try
                    {                       
                        var error = JsonSerializer.Deserialize<PlaidError>(ex.Response!, new JsonSerializerOptions()
                        {
                            PropertyNameCaseInsensitive = true,
                            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
                        })!;

                        if (error.Error_code != "PRODUCT_NOT_READY")
                            return Error(error);
                        else
                            // Wait a bit and try again
                            await Task.Delay(TimeSpan.FromSeconds(1));
                    }
                    catch
                    {
                        // Can't resolve. Re-throw the original exception
                        throw ex;
                    }
                }
            }

            if (response is null)
                throw new ApplicationException("Report unavailable after many retries");

            DataTable result = new ServerDataTable("Account", "Transactions/r", "Balance/r", "Days Available/r")
            {
                Rows = response.Report.Items
                    .SelectMany(x => x.Accounts.Select(a =>
                       new Row(
                           a.Name,
                           a.Transactions.Count.ToString(),
                           a.Balances.Current?.ToString("C2") ?? string.Empty,
                           a.Days_available.ToString("0")
                       ))
                    )
                    .ToArray()
            };
            return Ok(result);
        }
        catch (ApiException<Error> ex)
        {
            return Error(ex.Result);
        }
        catch (Exception ex)
        {
            return Error(ex);
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(DataTable), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PlaidError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Transfer()
    {
        try
        {
            var actrequest = new AccountsGetRequest() { Access_token = _credentials.AccessToken! };
            var actresponse = await _client.AccountsGetAsync(actrequest);

            var accountid = actresponse.Accounts.FirstOrDefault()?.Account_id;
            var transrequest = new TransferAuthorizationCreateRequest()
            {
                Access_token = _credentials.AccessToken!,
                Account_id = accountid!,
                Amount = "1.34",
                Network = TransferNetwork.Ach,
                Ach_class = ACHClass.Ppd,
                Type = TransferType.Credit,
                User = new()
                {
                    Legal_name = "Alice Cranberry",
                    Phone_number = "555-123-4567",
                    Email_address = "alice@example.com"
                }
            };
            var transresponse = await _client.TransferAuthorizationCreateAsync(transrequest);

            _logger.LogInformation($"Transfer Auth OK: {JsonSerializer.Serialize(transresponse)}");

            var authid = transresponse.Authorization.Id;
            var createrequest = new TransferCreateRequest()
            {
                Access_token = _credentials.AccessToken!,
                Idempotency_key = "1223abc456xyz7890001",
                Account_id = accountid!,
                Authorization_id = authid,
                Amount = "1.34",
                Network = TransferNetwork.Ach,
                Ach_class = ACHClass.Ppd,
                Type = TransferType.Credit,
                User = new()
                {
                    Legal_name = "Alice Cranberry",
                    Phone_number = "555-123-4567",
                    Email_address = "alice@example.com"
                }
            };
            var createresponse = await _client.TransferCreateAsync(createrequest);

            _logger.LogInformation($"Transfer Create OK: {JsonSerializer.Serialize(createresponse)}");

            var transferid = createresponse.Transfer.Id;
            var request = new TransferGetRequest()
            {
                Transfer_id = transferid,
            };
            var response = await _client.TransferGetAsync(request);

            DataTable result = new ServerDataTable("Transfer ID", "Amount/r", "Type", "ACH Class", "Network", "Status")
            {
                Rows = new Row[]
                {
                    new Row(
                        transferid,
                        response.Transfer.Amount,
                        response.Transfer.Type.ToString(),
                        response.Transfer.Ach_class.ToString(),
                        response.Transfer.Network.ToString(),
                        response.Transfer.Status.ToString()
                    )
                }
            };

            return Ok(result);
        }
        catch (ApiException<Error> ex)
        {
            return Error(ex.Result);
        }
        catch (Exception ex)
        {
            return Error(ex);
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(DataTable), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PlaidError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Verification()
    {
        try
        {
            var request = new IncomeVerificationPaystubsGetRequest() { Access_token = _credentials.AccessToken! };

            var response = await _client.IncomeVerificationPaystubsGetAsync(request);

            DataTable result = new ServerDataTable("Description", "Current Amount/r", "Currency")
            {
                Rows = response.Paystubs.SelectMany(x => x.Earnings.Breakdown.Select(y =>
                  new Row(
                    x.Employer + " " + y.Description,
                    y.Current_amount?.ToString("C2") ?? String.Empty,
                    y.Iso_currency_code ?? String.Empty
                  )
                ))
                .ToArray()
            };

            return Ok(result);
        }
        catch (ApiException<Error> ex)
        {
            return Error(ex.Result);
        }
        catch (Exception ex)
        {
            return Error(ex);
        }
    }

    ObjectResult Error(Error error, [CallerMemberName] string callerName = "")
    {
        var outerror = new ServerPlaidError(error);
        _logger.LogError($"{callerName}: {JsonSerializer.Serialize(outerror)}");

        return StatusCode(StatusCodes.Status400BadRequest, outerror);
    }

    ObjectResult Error(Exception ex, [CallerMemberName] string callerName = "")
    {
        _logger.LogError($"{callerName}: {ex.GetType().Name} {ex.Message}");
        return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
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

}


/// <summary>
/// Server-side version of shared plaid error
/// </summary>
internal class ServerPlaidError : PlaidWireError
{
    internal ServerPlaidError(Error error)
    {
        base.error_message = error.Error_message;
        base.display_message = error.Display_message;
        base.error_type = ToEnumString(error.Error_type);
        base.error_code = error.Error_code;
        base.error_type_path = _error_type_paths.GetValueOrDefault(base.error_type);
    }

    // The problem here is that the built-in JsonStringEnumConverter only converts
    // the enums into their C# representation, e.g. InvalidRequest. But when displaying
    // them to the user, we need to use the Plaid standard values, e.g. INVALID_REQUEST.
    // Those values are tied onto the Enum with an EnumMemberAttribute, so we could
    // create a custom converter. Or we could go the faster route, and just convert them
    // by hand here.

    // https://stackoverflow.com/questions/10418651/using-enummemberattribute-and-doing-automatic-string-conversions
    private static string ToEnumString<T>(T value)
    {
        var enumType = typeof(T);
        var name = Enum.GetName(enumType, value!);
        var enumMemberAttribute = ((EnumMemberAttribute[])enumType!.GetField(name!)!.GetCustomAttributes(typeof(EnumMemberAttribute), true)).Single();
        return enumMemberAttribute!.Value!;
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
