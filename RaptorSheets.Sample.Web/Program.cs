using RaptorSheets.Gig.Extensions;
using RaptorSheets.Sample.Web.Components;
using RaptorSheets.Sample.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Registers the factory only (no fixed options) - the spreadsheet ID and credentials come from
// user secrets, which won't exist on a fresh clone, so connecting happens lazily in
// GigConnectionProvider instead of failing at DI-resolution time.
builder.Services.AddRaptorSheetsGig();
builder.Services.AddScoped<GigConnectionProvider>();

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
