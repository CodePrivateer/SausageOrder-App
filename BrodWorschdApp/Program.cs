using BrodWorschdApp;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
// Add Database Service
builder.Services.AddScoped<DatabaseHandler>();
builder.Services.AddDbContext<DatabaseContext>();
// Add Localization Middleware
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportCultures = new List<CultureInfo> { new("en"), new("de") };
    options.DefaultRequestCulture = new RequestCulture("en");
    options.SupportedCultures = supportCultures;
    options.SupportedUICultures = supportCultures;
});
//Add Language Resource Service
builder.Services.AddSingleton<LanguageService>();
// Add session support
builder.Services.AddSession();

var app = builder.Build();

// Configure the language Middleware
var locOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>();
app.UseRequestLocalization(locOptions.Value);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.UseSession();

app.Run();
