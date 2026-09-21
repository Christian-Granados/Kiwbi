using System.Globalization;
using Kiwbi.Application;
using Kiwbi.Infrastructure;
using Kiwbi.Infrastructure.Persistence;
using Kiwbi.Web.DemoSeeding;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddScoped<DemoDataSeeder>();

// Enables header-based antiforgery validation for the buyer's HTMX selection requests (Feature 5.3), which are not <form> submits.
builder.Services.AddAntiforgery(options => options.HeaderName = "X-CSRF-TOKEN");

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

var app = builder.Build();

// Epic 11, Feature 11.6/11.7: apply pending EF Core migrations automatically at startup - a managed Postgres
// (Neon) has no way to run `dotnet ef database update` manually against the deployed instance. A no-op when
// the schema is already up to date, so it's safe to run unconditionally in every environment.
using (var migrationScope = app.Services.CreateScope())
{
    var dbContext = migrationScope.ServiceProvider.GetRequiredService<KiwbiDbContext>();
    await dbContext.Database.MigrateAsync();
}

// Feature 10.1: `dotnet run --project src/Kiwbi.Web -- --seed-demo` populates/resets the demo tenant, then exits
// without starting Kestrel.
if (args.Contains("--seed-demo"))
{
    using var seedScope = app.Services.CreateScope();
    var seeder = seedScope.ServiceProvider.GetRequiredService<DemoDataSeeder>();
    await seeder.SeedAsync();
    return;
}

// Force invariant number/date formatting (period decimals) regardless of the server's OS culture, since HTML5
// number inputs and model binding always use "." — only Razor's literal Spanish text is UI-language dependent.
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(CultureInfo.InvariantCulture),
    SupportedCultures = [CultureInfo.InvariantCulture],
    SupportedUICultures = [CultureInfo.InvariantCulture],
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

// Serves runtime-uploaded files (e.g. wwwroot/uploads) which MapStaticAssets' build-time manifest does not cover.
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
