using System.Reflection;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Tellurian.Trains.Schedules.Planning.App;
using Tellurian.Trains.Schedules.Planning.App.Translations;
using Tellurian.Trains.Schedules.Planning.Components;

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

var host = builder.Build();

// Apply the user's stored UI culture (set by the LanguageSelector) before the app runs.
await host.Services.ApplyStoredUiCultureAsync();

await host.RunAsync();
