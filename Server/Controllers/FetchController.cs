using Microsoft.AspNetCore.Mvc;
using PlaidQuickstartBlazor.Shared;

namespace PlaidQuickstartBlazor.Server.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
[Produces("application/json")]
public class FetchController : ControllerBase
{
    private readonly ILogger<FetchController> _logger;

    public FetchController(ILogger<FetchController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IList<string[]> Auth()
    {
        _logger.LogInformation("GetAuth");

        var result = new List<string[]>() 
        { 
            new[] { "A", "B", "C", "D", "E" },
            new[] { "1", "2", "3", "4", "5" },
            new[] { "1", "2", "3", "4", "5" },
            new[] { "1", "2", "3", "4", "5" },
        };

        return result;
    }
}
