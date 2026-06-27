namespace Tellurian.Trains.Schedules.Model.Validations;

/// <summary>
/// Rules for whether a model object may be deleted, and for deleting it. A delete is allowed only when
/// nothing in the <see cref="Plan"/> still references the object. The <see cref="Plan"/> is the aggregate
/// root, so every collection that could hold a reference is reachable from here.
/// </summary>
/// <remarks>
/// Each kind comes as a pair: <c>MayDelete</c> is a pure query (no mutation) used to enable a Delete
/// button and explain why it is disabled; <c>TryDelete</c> performs the removal when — and only when —
/// the query allows it. Neither persists: a <see cref="DeletionResult.Success"/> from <c>TryDelete</c>
/// obliges the caller to save and refresh (in the app, <c>ScheduleState.SaveAndNotify()</c>).
/// </remarks>
public static class DeletionRules
{
    extension(Plan plan)
    {
        /// <summary>
        /// Determines whether a <see cref="Company"/> may be deleted from the layout, i.e. no train,
        /// train category, vehicle or driver duty references it.
        /// </summary>
        public DeletionResult MayDelete(Company company)
        {
            var references = ReferencesTo(plan, company);
            return references.Count == 0
                ? new DeletionResult.Success(company)
                : new DeletionResult.Failure(company, references);
        }

        /// <summary>
        /// Deletes a <see cref="Company"/> from the layout when it is not referenced. Returns the
        /// <see cref="DeletionResult.Failure"/> from <c>MayDelete</c> unchanged when it is still
        /// referenced, leaving the model untouched.
        /// </summary>
        public DeletionResult TryDelete(Company company)
        {
            if (plan.MayDelete(company) is DeletionResult.Failure failure) return failure;
            plan.Layout.Companies.Remove(company);
            return new DeletionResult.Success(company);
        }

        /// <summary>
        /// Determines whether a <see cref="Country"/> may be removed from the layout's catalogue, i.e.
        /// it is not the default country and no company or region references it.
        /// </summary>
        public DeletionResult MayDelete(Country country)
        {
            var references = ReferencesTo(plan, country);
            return references.Count == 0
                ? new DeletionResult.Success(country)
                : new DeletionResult.Failure(country, references);
        }

        /// <summary>
        /// Removes a <see cref="Country"/> from the layout's catalogue when it is not referenced.
        /// Returns the <see cref="DeletionResult.Failure"/> from <c>MayDelete</c> unchanged when it is
        /// still referenced, leaving the model untouched.
        /// </summary>
        public DeletionResult TryDelete(Country country)
        {
            var countries = plan.Layout.Countries;
            if (plan.MayDelete(country) is DeletionResult.Failure failure) return failure;
            if (countries.FirstOrDefault(c => c.Id == country.Id) is { } existing)
                countries.Remove(existing);
            return new DeletionResult.Success(country);
        }
    }

    // Company is referenced by its Id (Train, ScheduledObject, DriverDuty foreign keys) and, for train
    // categories, by the held Company instance. Both are checked.
    private static List<Reference> ReferencesTo(Plan plan, Company company)
    {
        var references = new List<Reference>();
        foreach (var train in plan.Timetable.Trains.Where(t => References(t.CompanyId, t.Company, company)))
            references.Add(Reference.For(train));
        foreach (var category in plan.Timetable.TrainCategories.Where(c => c.Company?.Equals(company) ?? false))
            references.Add(Reference.For(category));
        // A vehicle's label comes from its ScheduledObjectType (Locomotive, Wagonset, …) via ITranslatable.
        foreach (var vehicle in plan.ScheduledObjects.Where(v => References(v.CompanyId, v.Company, company)))
            references.Add(Reference.For(vehicle));
        foreach (var duty in plan.DriverDuties.Where(d => References(d.CompanyId, d.Company, company)))
            references.Add(Reference.For(duty));
        return references;

        static bool References(int? companyId, Company? company, Company target) =>
            companyId == target.Id || (company?.Equals(target) ?? false);
    }

    // A country is referenced by the layout's default-country setting and by any company or region
    // assigned to it.
    private static List<Reference> ReferencesTo(Plan plan, Country country)
    {
        var layout = plan.Layout;
        var references = new List<Reference>();
        if (layout.Settings.Identity.DefaultCountryId == country.Id)
            references.Add(Reference.For(layout));
        foreach (var company in layout.Companies.Where(c => c.CountryId == country.Id))
            references.Add(Reference.For(company));
        // Region has no ToString override, so its display name is passed explicitly.
        foreach (var region in layout.Regions.Where(r => r.CountryId == country.Id))
            references.Add(new(nameof(Region), region.Name));
        return references;
    }
}
