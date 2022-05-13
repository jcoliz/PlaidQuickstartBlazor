using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PlaidApi;
using PlaidQuickstartBlazor.Shared;
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
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateLinkToken([FromQuery] bool? fix )
    {
        try
        {
            _logger.LogInformation($"CreateLinkToken (): {fix ?? false}");

            try
            {
                var request = new LinkTokenCreateRequest()
                {
                    Access_token = fix == true ? _credentials.AccessToken : null,
                    User = new LinkTokenCreateRequestUser { Client_user_id = Guid.NewGuid().ToString(), },
                    Client_name = "Quickstart for .NET",
                    Products = fix != true ? _credentials!.Products!.Split(',').Select(p => Enum.Parse<Products>(p, true)).ToArray() : Array.Empty<Products>(),
                    Language = "en", // TODO: Should pick up from config
                    Country_codes = _credentials!.CountryCodes!.Split(',').Select(p => Enum.Parse<CountryCode>(p, true)).ToArray(),
                };
                var response = await _client.LinkTokenCreateAsync(request);

                _logger.LogInformation($"CreateLinkToken OK: {JsonSerializer.Serialize(response)}");

                return Ok(response.Link_token);
            }
            catch (ApiException<Error> ex)
            {
                return Error(ex.Result);
            }
        }
        catch (Exception ex)
        {
            return Error(ex);
        }
    }

    [HttpPost]
    [IgnoreAntiforgeryToken(Order = 1001)]
    [ProducesResponseType(typeof(PlaidCredentials), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ExchangePublicToken(LinkResult link)
    {
        // Yes, we send WAY more information than needed here. That's so we can log it!

        _logger.LogInformation($"ExchangePublicToken (): {JsonSerializer.Serialize(link)}");

        try
        {
            var request = new ItemPublicTokenExchangeRequest()
            {
                Public_token = link.public_token
            };
            var response = await _client.ItemPublicTokenExchangeAsync(request);

            _credentials.AccessToken = response.Access_token;
            _credentials.ItemId = response.Item_id;

            _logger.LogInformation($"ExchangePublicToken OK: {JsonSerializer.Serialize(_credentials)}");

            return Ok(_credentials);
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

    [HttpPost]
    [IgnoreAntiforgeryToken(Order = 1001)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult LinkFail(LinkResult link)
    {
        // We're logging failures in case you want to come back and look for them later

        _logger.LogInformation($"LinkFail (): {JsonSerializer.Serialize(link)}");

        return Ok();
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

}

