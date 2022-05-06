using Going.Plaid;
using Going.Plaid.Entity;
using Going.Plaid.Item;
using Going.Plaid.Link;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PlaidQuickstartBlazor.Shared;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace PlaidQuickstartBlazor.Server.Controllers;

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

    [HttpGet]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateLinkToken()
    {
        var response = await _client.LinkTokenCreateAsync(
            new LinkTokenCreateRequest()
            {
                User = new LinkTokenCreateRequestUser { ClientUserId = Guid.NewGuid().ToString(), },
                ClientName = "Quickstart for .NET",
                Products = _credentials!.Products!.Split(',').Select(p => Enum.Parse<Products>(p, true)).ToArray(),
                Language = Language.English,
                CountryCodes = _credentials!.CountryCodes!.Split(',').Select(p => Enum.Parse<CountryCode>(p, true)).ToArray(),
            });

        if (response.Error is not null)
            return Error(response.Error);

        _logger.LogInformation($"CreateLinkToken OK: {JsonSerializer.Serialize(response.LinkToken)}");

        return Ok(response.LinkToken);
    }

    [HttpPost]
    [IgnoreAntiforgeryToken(Order = 1001)]
    [ProducesResponseType(typeof(PlaidCredentials), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExchangePublicToken(LinkResult link)
    {
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

    ObjectResult Error(Going.Plaid.Errors.PlaidError error, [CallerMemberName] string callerName = "")
    {
        _logger.LogError($"{callerName}: {JsonSerializer.Serialize(error)}");
        return StatusCode(StatusCodes.Status400BadRequest, error);
    }
}
