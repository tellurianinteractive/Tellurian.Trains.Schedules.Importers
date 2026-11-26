using System.Reflection;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Tellurian.Trains.Schedules.Planning.App;
using Tellurian.Trains.Schedules.Planning.App.Translations;
using Tellurian.Trains.Schedules.Planning.Components;
using Tellurian.Trains.Schedules.Planning.Components.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Host-specific: the base address is where this host serves its static assets from.
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Host-specific: the app's version, stamped into this assembly by the <Version> property in
// Planning.App.csproj. That is the single place it is bumped for a release.
var appVersion = Assembly.GetExecutingAssembly()
    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "";

// Everything else lives in the Planning.Components library, shared with any other host.
builder.Services.AddPlanningComponents(new AppInfoSettings { Version = appVersion });

// Host-specific: errors are kept in this browser's localStorage so the user can report them, also after a
// reload. Logged errors are caught by the logging provider; these two catch what no one logs.
builder.Services.AddErrorReports();

var host = builder.Build();

var errorReports = host.Services.GetRequiredService<ErrorReports>();
AppDomain.CurrentDomain.UnhandledException += (_, e) =>
    errorReports.Add("Unhandled exception", (e.ExceptionObject as Exception)?.Message ?? "", e.ExceptionObject as Exception);
TaskScheduler.UnobservedTaskException += (_, e) =>
    errorReports.Add("Unobserved task exception", e.Exception.GetBaseException().Message, e.Exception);

// Apply the user's stored UI culture (set by the LanguageSelector) before the app runs.
await host.Services.ApplyStoredUiCultureAsync();

// Reports print in the layout's languages, not only the user's, so every language's translations are needed.
await LanguageResources.LoadAllSupportedLanguagesAsync();

await host.RunAsync();
