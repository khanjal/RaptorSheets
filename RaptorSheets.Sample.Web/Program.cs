using RaptorSheets.Gig.Extensions;
using RaptorSheets.Home.Extensions;
using RaptorSheets.Job.Extensions;
using RaptorSheets.Sample.Web.Components;
using RaptorSheets.Sample.Web.Services;
using RaptorSheets.Stock.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Registers the factory only (no fixed options), per domain - the spreadsheet ID and credentials
// come from user secrets, which won't exist on a fresh clone, so connecting happens lazily in each
// domain's XSheetOperations instead of failing at DI-resolution time.
builder.Services.AddRaptorSheetsGig();
builder.Services.AddRaptorSheetsStock();
builder.Services.AddRaptorSheetsJob();
builder.Services.AddRaptorSheetsHome();

// GigSheetOperations is also registered under its own concrete type, not just ISheetOperations,
// since Home.razor's Gig-only setup wizard needs the strongly-typed manager for
// CreateAllSheets/GenerateDemoData - both resolve to the same scoped instance either way.
builder.Services.AddScoped<GigSheetOperations>();
builder.Services.AddScoped<ISheetOperations>(sp => sp.GetRequiredService<GigSheetOperations>());
// StockSheetOperations is deliberately not registered here yet - RaptorSheets.Stock's entities
// (StockEntity/PriceEntity/CostEntity/...) have no [Column] attributes at all (unlike Gig/Job/Home),
// so this generic reflection-driven UI can't discover any columns for them; every Stock sheet would
// render with no data, just the Delete button. That's a gap in the Stock library itself (tracked as
// a follow-up), not something to work around here - see docs/SAMPLE-APP.md. The class is otherwise
// complete and ready to register as ISheetOperations once Stock's entities get [Column] attributes.
builder.Services.AddScoped<ISheetOperations, JobSheetOperations>();
builder.Services.AddScoped<ISheetOperations, HomeSheetOperations>();
builder.Services.AddScoped<DomainRegistry>();

builder.Services.AddSingleton<ReferenceSheetCache>();

// ConfigurationManager (builder.Configuration) already *is* the IConfigurationRoot the app runs
// on - registering it lets UserSecretsWriter call .Reload() after writing new secrets, so a setup
// submission takes effect immediately instead of needing an app restart.
builder.Services.AddSingleton<IConfigurationRoot>(builder.Configuration);
builder.Services.AddSingleton<UserSecretsWriter>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

await app.RunAsync();
