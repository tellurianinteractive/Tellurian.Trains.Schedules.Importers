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

        /// <summary>
        /// Determines whether a <see cref="Train"/> may be deleted from the timetable, i.e. no vehicle
        /// schedule or driver duty runs any part of it.
        /// </summary>
        public DeletionResult MayDelete(Train train)
        {
            var references = ReferencesTo(plan, train);
            return references.Count == 0
                ? new DeletionResult.Success(train)
                : new DeletionResult.Failure(train, references);
        }

        /// <summary>
        /// Deletes a <see cref="Train"/> from the timetable when nothing runs it: removes the train's
        /// calls from the tracks that hold them, and removes the train (its calls go with it). Returns
        /// the <see cref="DeletionResult.Failure"/> from <c>MayDelete</c> unchanged when it is still
        /// referenced, leaving the model untouched.
        /// </summary>
        public DeletionResult TryDelete(Train train)
        {
            if (plan.MayDelete(train) is DeletionResult.Failure failure) return failure;
            foreach (var call in train.Calls) call.Track.Calls.Remove(call);
            plan.Timetable.Trains.Remove(train);
            return new DeletionResult.Success(train);
        }

        /// <summary>
        /// Determines whether a <see cref="StationCall"/> may be deleted from its train, i.e. no vehicle
        /// schedule or driver duty part starts or ends at it.
        /// </summary>
        public DeletionResult MayDelete(StationCall call)
        {
            var references = ReferencesTo(plan, call);
            return references.Count == 0
                ? new DeletionResult.Success(call)
                : new DeletionResult.Failure(call, references);
        }

        /// <summary>
        /// Deletes a <see cref="StationCall"/> from its train when no part uses it: removes the call from
        /// its train and its track. Returns the <see cref="DeletionResult.Failure"/> from
        /// <c>MayDelete</c> unchanged when it is still referenced, leaving the model untouched.
        /// </summary>
        public DeletionResult TryDelete(StationCall call)
        {
            if (plan.MayDelete(call) is DeletionResult.Failure failure) return failure;
            var train = call.Train;
            train.Calls.Remove(call);
            call.Track.Calls.Remove(call);
            return new DeletionResult.Success(call);
        }

        /// <summary>
        /// A vehicle <see cref="Schedule"/> may always be deleted: it is a planner construct that nothing
        /// outside it references (its assignments and parts are owned by it).
        /// </summary>
        public DeletionResult MayDelete(Schedule schedule) => new DeletionResult.Success(schedule);

        /// <summary>
        /// Deletes a vehicle <see cref="Schedule"/>: unassigns its vehicles (removes each
        /// <see cref="ScheduleAssignment"/> from its <see cref="ScheduledObject"/>), detaches its train
        /// parts, and removes it from the plan. The vehicles themselves stay in the pool.
        /// </summary>
        public DeletionResult TryDelete(Schedule schedule)
        {
            foreach (var assignment in schedule.Vehicles
                .SelectMany(v => v.ScheduleAssignments.Where(a => schedule.Equals(a.Schedule)).ToList()))
                assignment.ScheduledObject.ScheduleAssignments.Remove(assignment);
            foreach (var part in schedule.Parts.ToList())
            {
                part.Schedule = null;
                part.ScheduleId = null;
            }
            schedule.Parts.Clear();
            plan.Schedules.Remove(schedule);
            return new DeletionResult.Success(schedule);
        }

        /// <summary>
        /// A <see cref="ScheduleAssignment"/> may always be removed: unassigning a vehicle from a schedule
        /// references nothing else.
        /// </summary>
        public DeletionResult MayDelete(ScheduleAssignment assignment) => new DeletionResult.Success(assignment);

        /// <summary>
        /// Unassigns a vehicle by removing the <see cref="ScheduleAssignment"/> from its
        /// <see cref="ScheduledObject"/>. The vehicle stays in the pool for reuse; the schedule and its
        /// parts are untouched.
        /// </summary>
        public DeletionResult TryDelete(ScheduleAssignment assignment)
        {
            assignment.ScheduledObject.ScheduleAssignments.Remove(assignment);
            return new DeletionResult.Success(assignment);
        }

        /// <summary>
        /// A <see cref="DriverDuty"/> may always be deleted: it is a planner construct that nothing outside
        /// it references (its parts are owned by the vehicle schedules and only referenced here; its notes
        /// are owned by it).
        /// </summary>
        public DeletionResult MayDelete(DriverDuty duty) => new DeletionResult.Success(duty);

        /// <summary>
        /// Deletes a <see cref="DriverDuty"/>: releases its train parts (they stay owned by their vehicle
        /// schedules and may still be worked by other duties) and removes it from the plan.
        /// </summary>
        public DeletionResult TryDelete(DriverDuty duty)
        {
            duty.Parts.Clear();
            plan.DriverDuties.Remove(duty);
            return new DeletionResult.Success(duty);
        }

        /// <summary>
        /// Determines whether a <see cref="CargoFlowOptions"/> description may be removed from the
        /// timetable catalogue, i.e. no cargo flow on any train references it.
        /// </summary>
        public DeletionResult MayDelete(CargoFlowOptions options)
        {
            var references = ReferencesTo(plan, options);
            return references.Count == 0
                ? new DeletionResult.Success(options)
                : new DeletionResult.Failure(options, references);
        }

        /// <summary>
        /// Removes a <see cref="CargoFlowOptions"/> description from the timetable catalogue when no
        /// cargo flow references it. Returns the <see cref="DeletionResult.Failure"/> from
        /// <c>MayDelete</c> unchanged when it is still referenced, leaving the model untouched.
        /// </summary>
        public DeletionResult TryDelete(CargoFlowOptions options)
        {
            if (plan.MayDelete(options) is DeletionResult.Failure failure) return failure;
            plan.Timetable.CargoFlowOptions.Remove(options);
            return new DeletionResult.Success(options);
        }

        /// <summary>
        /// A <see cref="CargoFlowTrainPart"/> may always be deleted: nothing else references it.
        /// </summary>
        public DeletionResult MayDelete(CargoFlowTrainPart cargoFlow) => new DeletionResult.Success(cargoFlow);

        /// <summary>
        /// Deletes a <see cref="CargoFlowTrainPart"/> from the train that owns it.
        /// </summary>
        public DeletionResult TryDelete(CargoFlowTrainPart cargoFlow)
        {
            // The owning train is derived from the cargo flow's from-call; find it through the timetable
            // to stay consistent with how wagon groups are removed.
            plan.Timetable.Trains.FirstOrDefault(t => t.CargoFlows.Contains(cargoFlow))?.CargoFlows.Remove(cargoFlow);
            return new DeletionResult.Success(cargoFlow);
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

    // A train is referenced by any vehicle-schedule or driver-duty part that runs it.
    private static List<Reference> ReferencesTo(Plan plan, Train train)
    {
        var references = new List<Reference>();
        foreach (var schedule in plan.Schedules.Where(s => s.Parts.Any(p => p.Train.Equals(train))))
            references.Add(Reference.For(schedule));
        foreach (var duty in plan.DriverDuties.Where(d => d.Parts.Any(p => p.Train.Equals(train))))
            references.Add(Reference.For(duty));
        return references;
    }

    // A station call is referenced by any vehicle-schedule or driver-duty part that starts or ends at it.
    private static List<Reference> ReferencesTo(Plan plan, StationCall call)
    {
        var references = new List<Reference>();
        foreach (var schedule in plan.Schedules.Where(s => s.Parts.Any(p => p.From.Equals(call) || p.To.Equals(call))))
            references.Add(Reference.For(schedule));
        foreach (var duty in plan.DriverDuties.Where(d => d.Parts.Any(p => p.From.Equals(call) || p.To.Equals(call))))
            references.Add(Reference.For(duty));
        return references;
    }

    // A cargo flow description is referenced by any train carrying a cargo flow that uses it (matched by
    // the foreign key, or by instance for an as-yet-unsaved flow).
    private static List<Reference> ReferencesTo(Plan plan, CargoFlowOptions options)
    {
        var references = new List<Reference>();
        foreach (var train in plan.Timetable.Trains.Where(t => t.CargoFlows.Any(cf => cf.CargoFlowOptionsId == options.Id || ReferenceEquals(cf.CargoFlowOptions, options))))
            references.Add(Reference.For(train));
        return references;
    }
}
