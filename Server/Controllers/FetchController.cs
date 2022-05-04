using Microsoft.AspNetCore.Mvc;
using PlaidQuickstartBlazor.Shared;

namespace PlaidQuickstartBlazor.Server.Controllers;

[ApiController]
[Route("[controller]/[action]")]
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
        _logger.LogInformation("Auth");

        var result = new List<string[]>() 
        { 
            new[] { "A", "B", "C", "D", "E" },
            new[] { "1", "2", "3", "4", "5" },
            new[] { "1", "2", "3", "4", "5" },
            new[] { "1", "2", "3", "4", "5" },
        };

        return result;
    }
    [HttpGet]
    public IList<string[]> Transactions()
    {
        _logger.LogInformation("Transactions");

        var result = new List<string[]>()
        {
            new[] { "A", "B", "C", "D", "E" },
            new[] { "1", "2", "3", "4", "5" },
            new[] { "1", "2", "3", "4", "5" },
            new[] { "1", "2", "3", "4", "5" },
        };

        return result;
    }
    [HttpGet]
    public IList<string[]> Identity()
    {
        _logger.LogInformation("Identity");

        var result = new List<string[]>()
        {
            new[] { "A", "B", "C", "D", "E" },
            new[] { "1", "2", "3", "4", "5" },
            new[] { "1", "2", "3", "4", "5" },
            new[] { "1", "2", "3", "4", "5" },
        };

        return result;
    }
    [HttpGet]
    public IList<string[]> Holdings()
    {
        _logger.LogInformation("Holdings");

        var result = new List<string[]>()
        {
            new[] { "A", "B", "C", "D", "E" },
            new[] { "1", "2", "3", "4", "5" },
            new[] { "1", "2", "3", "4", "5" },
            new[] { "1", "2", "3", "4", "5" },
        };

        return result;
    }
    [HttpGet]
    public IList<string[]> Investments_Transactions()
    {
        _logger.LogInformation("Investments_Transactions");

        var result = new List<string[]>()
        {
            new[] { "A", "B", "C", "D", "E" },
            new[] { "1", "2", "3", "4", "5" },
            new[] { "1", "2", "3", "4", "5" },
            new[] { "1", "2", "3", "4", "5" },
        };

        return result;
    }
    [HttpGet]
    public IList<string[]> Balance()
    {
        _logger.LogInformation("Balance");

        var result = new List<string[]>()
        {
            new[] { "A", "B", "C", "D", "E" },
            new[] { "1", "2", "3", "4", "5" },
            new[] { "1", "2", "3", "4", "5" },
            new[] { "1", "2", "3", "4", "5" },
        };

        return result;
    }
    [HttpGet]
    public IList<string[]> Liabilities()
    {
        _logger.LogInformation("Liabilities");

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
