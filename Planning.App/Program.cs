using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using System.Globalization;
using Tellurian.Localization;
using Tellurian.Localization.DependencyInjection;
using Tellurian.Trains.Schedules.Importers.Interfaces;
using Tellurian.Trains.Schedules.Planning.App;
using Tellurian.Trains.Schedules.Planning.App.Components;
using Tellurian.Trains.Schedules.Planning.App.Services;
using Tellurian.Trains.Schedules.Planning.App.Translations;
using Tellurian.Trains.Schedules.Planning.App.Translations.Resources;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddSingleton<BrowserStorageService>();
builder.Services.AddSingleton<UiPreferenceService>();
builder.Services.AddSingleton<DockLayoutState>();
builder.Services.AddSingleton<ScheduleStateService>();
builder.Services.AddScoped<ICompaniesService, CompaniesService>();
builder.Services.AddScoped<ITrainCategoriesService, TrainCategoriesService>();
builder.Services.AddScoped<ScheduleImportService>();
builder.Services.AddScoped<ScheduleExportService>();
builder.Services.AddScoped<HelpContentService>();

// Localisation — register language service, RESX and Markdown providers.
// NOTE: Language.IsFallback has 'internal set' in Tellurian.Localization 1.0.1,
// so we use the individual registration methods until that is changed to 'init'.
builder.Services.AddLanguageService(LanguageService.SupportedLanguages);
builder.Services.AddResxResourceProviders([typeof(Labels)]);
builder.Services.AddHttpMarkdownResourceProvider("_content/Tellurian.Trains.Schedules.Planning.App.Translations/Content");
builder.Services.AddObjectResourceProvider();
builder.Services.AddScoped<TranslationService>();
builder.Services.AddResxStringLocalizers();

var host = builder.Build();

// Apply the user's stored UI culture (set by the LanguageSelector) before the app runs.
await host.Services.ApplyStoredUiCultureAsync();

await host.RunAsync();
