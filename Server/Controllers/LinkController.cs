using Going.Plaid;
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
}
