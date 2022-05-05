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

    private DataTable SampleResult => new DataTable()
    {
        Columns = (new[] { "A", "B", "C", "D", "E" })
            .Select(x => new Column() { Title = x })
            .ToArray(),

        Rows = new[]
            {
                new Row() { Cells = new[] { "1", "2", "3", "4", "5" } },
                new Row() { Cells = new[] { "1", "2", "3", "4", "5" } },
                new Row() { Cells = new[] { "1", "2", "3", "4", "5" } },
                new Row() { Cells = new[] { "1", "2", "3", "4", "5" } },
            }
    };

    [HttpGet]
    public DataTable Auth()
    {
        _logger.LogInformation("Auth");

        return SampleResult;
    }
    [HttpGet]
    public DataTable Transactions()
    {
        _logger.LogInformation("Transactions");

        return SampleResult;
    }
    [HttpGet]
    public DataTable Identity()
    {
        _logger.LogInformation("Identity");

        return SampleResult;
    }
    [HttpGet]
    public DataTable Holdings()
    {
        _logger.LogInformation("Holdings");

        return SampleResult;
    }
    [HttpGet]
    public DataTable Investments_Transactions()
    {
        _logger.LogInformation("Investments_Transactions");

        return SampleResult;
    }
    [HttpGet]
    public DataTable Balance()
    {
        _logger.LogInformation("Balance");

        return SampleResult;
    }
    [HttpGet]
    public DataTable Liabilities()
    {
        _logger.LogInformation("Liabilities");

        return SampleResult;
    }
}
