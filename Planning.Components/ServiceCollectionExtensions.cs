using Tellurian.Localization;
using Tellurian.Localization.DependencyInjection;
using Tellurian.Trains.Schedules.Importers.Interfaces;
using Tellurian.Trains.Schedules.Planning.App.Translations;
using Tellurian.Trains.Schedules.Planning.App.Translations.Resources;
using Tellurian.Trains.Schedules.Planning.Components.Docking;
using Tellurian.Trains.Schedules.Planning.Components.Services;

namespace Tellurian.Trains.Schedules.Planning.Components;

/// <summary>
/// Registers everything the components in this library need, so that any host — the WebAssembly
/// app today, an interactive Server app for online collaboration later — wires them up identically.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the component services and the localisation providers.
    /// </summary>
    /// <param name="services">The service collection to add to.</param>
    /// <param name="appInfo">Facts about the hosting application, such as its release version.</param>
    /// <remarks>
    /// The host still owns what only the host can know: the <see cref="HttpClient"/> and its base
    /// address, the root components, its own version, and applying the stored UI culture at startup.
    /// </remarks>
    public static IServiceCollection AddPlanningComponents(this IServiceCollection services, AppInfoSettings appInfo)
    {
        // Immutable and identical for every user, so a singleton is correct here even under a
        // multi-user host.
        services.AddSingleton(appInfo);


        // Every service below holds one user's state — the open plan, the dock layout, UI
        // preferences, validation results. They are therefore Scoped, never Singleton.
        //
        // Under WebAssembly there is a single scope per browser tab, so Scoped and Singleton
        // behave the same. Under interactive Server a scope is one SignalR circuit, i.e. one
        // user's session, while a singleton is shared by every connected user — which would
        // leak one user's plan and dock layout to everyone else. Scoped is correct in both,
        // so the shared-data model for collaboration is introduced deliberately (through a
        // store behind an interface) rather than by accident through a service lifetime.
        services.AddScoped<BrowserStorageService>();
        services.AddScoped<CrossWindowSyncService>();
        services.AddScoped<UiPreferenceService>();
        services.AddScoped<DockLayoutState>();
        services.AddScoped<ScheduleStateService>();
        services.AddScoped<ValidationStateService>();
        services.AddScoped<ICompaniesService, CompaniesService>();
        services.AddScoped<ITrainCategoriesService, TrainCategoriesService>();
        services.AddScoped<ScheduleImportService>();
        services.AddScoped<ScheduleExportService>();
        services.AddScoped<ModuleRegistryUploadService>();
        services.AddScoped<HelpContentService>();

        // Localisation — language service, RESX and Markdown providers. Registered one by one rather
        // than through AddTellurianLocalization, which registers the file-based markdown provider; a
        // WebAssembly host has no file system, so the content is fetched over HTTP instead.
        services.AddLanguageService(LanguageService.SupportedLanguages);
        services.AddResxResourceProviders([typeof(Labels)]);
        services.AddHttpMarkdownResourceProvider(
            "_content/Tellurian.Trains.Schedules.Planning.App.Translations/Content",
            LanguageService.NeutralLanguage);
        services.AddObjectResourceProvider();
        services.AddScoped<TranslationService>();
        services.AddResxStringLocalizers();

        return services;
    }
}
