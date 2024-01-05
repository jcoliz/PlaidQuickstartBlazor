using Going.Plaid;
using Going.Plaid.Entity;
using Going.Plaid.Item;
using Going.Plaid.Link;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PlaidQuickstartBlazor.Shared;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace PlaidQuickstartBlazor.Server.Controllers;

/// <summary>
/// Manage the login process to the Plaid service
/// </summary>
/// <seealso href="https://plaid.com/docs/link/"/>
/// <remarks>
/// Handles all of the traffic from the Link component
/// </remarks>
[ApiController]
[Route("[controller]/[action]")]
[Produces("application/json")]
public class LinkController : ControllerBase
{
    private readonly ILogger<LinkController> _logger;
    private readonly PlaidCredentials _credentials;
    private readonly PlaidClient _client;

    public LinkController(ILogger<LinkController> logger, IOptions<PlaidCredentials> credentials, PlaidClient client)
    {
        _logger = logger;
        _credentials = credentials.Value;
        _client = client;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PlaidCredentials), StatusCodes.Status200OK)]
    public ActionResult Info()
    {
        _logger.LogInformation($"Info OK: {JsonSerializer.Serialize(_credentials)}");

        return Ok(_credentials);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult Logout()
    {
        _logger.LogInformation($"Logout OK");

        _credentials.AccessToken = null;
        _credentials.ItemId = null;

        return Ok();
    }

    [HttpGet]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateLinkToken([FromQuery] bool? fix )
    {
        try
        {
            _logger.LogInformation($"CreateLinkToken (): {fix ?? false}");

            var response = await _client.LinkTokenCreateAsync(
                new LinkTokenCreateRequest()
                {
                    AccessToken = fix == true ? _credentials.AccessToken : null,
                    User = new LinkTokenCreateRequestUser { ClientUserId = Guid.NewGuid().ToString(), },
                    ClientName = "Quickstart for .NET",
                    Products = fix != true ? _credentials!.Products!.Split(',').Select(p => Enum.Parse<Products>(p, true)).ToArray() : Array.Empty<Products>(),
                    Language = CultureInfo.CurrentCulture.TwoLetterISOLanguageName ?? "en",
                    CountryCodes = _credentials!.CountryCodes!.Split(',').Select(p => Enum.Parse<CountryCode>(p, true)).ToArray(),
                });

            if (response.Error is not null)
                return Error(response.Error);

            _logger.LogInformation($"CreateLinkToken OK: {JsonSerializer.Serialize(response.LinkToken)}");

            return Ok(response.LinkToken);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [HttpPost]
    [IgnoreAntiforgeryToken(Order = 1001)]
    [ProducesResponseType(typeof(PlaidCredentials), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExchangePublicToken(LinkResult link)
    {
        // Yes, we send WAY more information than needed here. That's so we can log it!

        _logger.LogInformation($"ExchangePublicToken (): {JsonSerializer.Serialize(link)}");

        var request = new ItemPublicTokenExchangeRequest()
        {
            PublicToken = link.public_token
        };

        var response = await _client.ItemPublicTokenExchangeAsync(request);

        if (response.Error is not null)
            return Error(response.Error);

        _credentials.AccessToken = response.AccessToken;
        _credentials.ItemId = response.ItemId;

        _logger.LogInformation($"ExchangePublicToken OK: {JsonSerializer.Serialize(_credentials)}");

        return Ok(_credentials);
    }

    [HttpPost]
    [IgnoreAntiforgeryToken(Order = 1001)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult LinkFail(LinkResult link)
    {
        // We're logging failures in case you want to come back and look for them later

        _logger.LogInformation($"LinkFail (): {JsonSerializer.Serialize(link)}");

        return Ok();
    }

    ObjectResult Error(Going.Plaid.Entity.PlaidError error, [CallerMemberName] string callerName = "")
    {
        _logger.LogError($"{callerName}: {JsonSerializer.Serialize(error)}");
        return StatusCode(StatusCodes.Status400BadRequest, error);
    }
}
