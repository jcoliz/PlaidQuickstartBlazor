using Going.Plaid;
using Going.Plaid.Entity;
using Going.Plaid.Item;
using Going.Plaid.Link;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PlaidQuickstartBlazor.Shared;
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
    public ActionResult Info()
    {
        _logger.LogInformation($"Info OK: {JsonSerializer.Serialize(_credentials)}");

        return Ok(_credentials);
    }

    [HttpGet]
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
        {
            _logger.LogError($"CreateLinkToken {response.StatusCode}: {JsonSerializer.Serialize(response.Error)}");
            return StatusCode((int)response.StatusCode, response.Error);
        }

        _logger.LogInformation($"CreateLinkToken OK: {JsonSerializer.Serialize(response.LinkToken)}");

        return Ok(response.LinkToken);
    }

    [HttpPost]
    [IgnoreAntiforgeryToken(Order = 1001)]
    public async Task<IActionResult> ExchangePublicToken(LinkResult link)
    {
        _logger.LogInformation($"ExchangePublicToken (): {JsonSerializer.Serialize(link)}");

        var request = new ItemPublicTokenExchangeRequest()
        {
            PublicToken = link.public_token
        };

        var response = await _client.ItemPublicTokenExchangeAsync(request);

        if (response.Error is not null)
        {
            _logger.LogError($"ExchangePublicToken {response.StatusCode}: {JsonSerializer.Serialize(response.Error)}");
            return StatusCode((int)response.StatusCode, response.Error);
        }

        _credentials.AccessToken = response.AccessToken;
        _credentials.ItemId = response.ItemId;

        _logger.LogInformation($"ExchangePublicToken OK: {JsonSerializer.Serialize(_credentials)}");

        return Ok(_credentials);
    }
}
