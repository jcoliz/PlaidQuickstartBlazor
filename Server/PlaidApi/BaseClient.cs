using Microsoft.Extensions.Options;
using System.Text.Json;

namespace PlaidApi;

public class PlaidClientBase
{
    private readonly PlaidApiOptions _options;

    public bool Verbose { get; set; }

    public PlaidClientBase(IOptions<PlaidApiOptions> options)
    {
        _options = options.Value;
    }

    protected Task<HttpRequestMessage> CreateHttpRequestMessageAsync(CancellationToken cancellationToken)
    {
        var result = new HttpRequestMessage();

        if (_options.ClientId is not null)
            result.Headers.Add("PLAID-CLIENT-ID", _options.ClientId);

        if (_options.Secret is not null)
            result.Headers.Add("PLAID-SECRET", _options.Secret);

        return Task.FromResult(result);
    }

    protected string BaseUrl => _options.Environment is not null ? $"https://{_options.Environment.ToLowerInvariant()}.plaid.com" : throw new ApplicationException();

    protected async Task PrepareRequestAsync(System.Net.Http.HttpClient client, System.Net.Http.HttpRequestMessage request, string url)
    {
        if (Verbose)
        {
            Stream stream = await request!.Content!.ReadAsStreamAsync();
            using var sr = new StreamReader(stream);
            var tx = sr.ReadToEnd();
            stream.Seek(0, SeekOrigin.Begin);
            Console.WriteLine($"\n[>>>]: {url} {tx}\n");
        }
    }
    protected Task PrepareRequestAsync(System.Net.Http.HttpClient client, System.Net.Http.HttpRequestMessage request, System.Text.StringBuilder builder)
    {
        return Task.CompletedTask;
    }

    protected async Task ProcessResponseAsync(System.Net.Http.HttpClient client, System.Net.Http.HttpResponseMessage response, CancellationToken token)
    {
        if (Verbose)
        {
            // Not 'using' it, as I am passing it into streamcontent below
            var stream = new MemoryStream();
            await response!.Content!.CopyToAsync(stream, token);
            stream.Seek(0, SeekOrigin.Begin);

            // Not 'using' it, as it will dispose the underlying stream
            var sr = new StreamReader(stream);
            var tx = sr.ReadToEnd();
            Console.WriteLine($"[<<<]: {response.StatusCode} {tx}\n");

            // Replace the content, now that we already read it out.
            stream.Seek(0, SeekOrigin.Begin);
            response.Content = new StreamContent(stream);
        }
    }

    protected void UpdateJsonSerializerSettings(Newtonsoft.Json.JsonSerializerSettings settings)
    {
    }

}

public class PlaidApiOptions
{
    public const string SectionKey = "Plaid";

    public string? ClientId { get; set; }
    public string? Secret { get; set; }
    public string? Environment { get; set; }
}
