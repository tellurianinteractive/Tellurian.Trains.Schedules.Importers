using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Tellurian.Localization.DependencyInjection;
using Tellurian.Trains.Schedules.Importers.Interfaces;
using Tellurian.Trains.Schedules.Planning.App;
using Tellurian.Trains.Schedules.Planning.App.Services;
using Tellurian.Trains.Schedules.Planning.App.Translations.Resources;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddSingleton<PaneState>();
builder.Services.AddSingleton<ScheduleState>();
builder.Services.AddScoped<ICompaniesService, WasmCompaniesService>();
builder.Services.AddScoped<ITrainCategoriesService, WasmTrainCategoriesService>();
builder.Services.AddScoped<ScheduleImportService>();

// Localisation — register language service, RESX and Markdown providers.
// NOTE: Language.IsFallback has 'internal set' in Tellurian.Localization 1.0.1,
// so we use the individual registration methods until that is changed to 'init'.
builder.Services.AddLanguageService(
[
    new("en", true) { CultureCode = "GB" },
    new("sv", true) { CultureCode = "SE" },
    new("de", false) { CapitalizesNouns = true },
    new("da", false),
    new("nb", false),
]);
builder.Services.AddResxResourceProviders([typeof(Strings)]);
builder.Services.AddHttpMarkdownResourceProvider("Content");
builder.Services.AddObjectResourceProvider();

await builder.Build().RunAsync();
