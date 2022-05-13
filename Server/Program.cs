using PlaidApi;
using PlaidQuickstartBlazor.Shared;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// You can put your plaid secrets here. But really you can put them
// ANYWHERE in the .NET configuration system
builder.Configuration.AddYamlFile("secrets.yaml",optional:true);

// Add services to the container.

builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(namingPolicy:null));
    });

builder.Services.AddRazorPages();

// Add Plaid API services
builder.Services.AddHttpClient();
builder.Services.Configure<PlaidCredentials>(builder.Configuration.GetSection(PlaidApiOptions.SectionKey));
builder.Services.Configure<PlaidApiOptions>(builder.Configuration.GetSection(PlaidApiOptions.SectionKey));
builder.Services.AddSingleton<PlaidClient>();
builder.Services.AddSingleton<ContextContainer>(new ContextContainer() { RunningOnServer = true });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

// https://andrewlock.net/enabling-prerendering-for-blazor-webassembly-apps/
app.UseEndpoints(endpoints =>
{
    endpoints.MapRazorPages();
    endpoints.MapFallbackToPage("/_Host");
});

//app.MapRazorPages();
app.MapControllers();

app.Run();
