using Going.Plaid;
using Going.Plaid.Entity;
using Going.Plaid.Link;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PlaidQuickstartBlazor.Shared;

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
        _client.AccessToken = _credentials.AccessToken;
    }

    [HttpGet]
    public ActionResult Info()
    {
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
            return StatusCode((int)response.StatusCode, response.Error.ErrorMessage);

        return Ok(response.LinkToken);
    }

}
