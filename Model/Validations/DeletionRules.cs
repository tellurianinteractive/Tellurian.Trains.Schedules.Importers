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
        /// Determines whether a <see cref="StationCall"/> may be deleted from its train: only the train's
        /// first and last call may go, and only when no vehicle schedule or driver duty part starts or
        /// ends at it. A train calls at every operating location along its route, so a call in between is
        /// not the train's to skip — deleting one would leave a gap in the route. Shortening the route at
        /// either end is what deleting a call means.
        /// </summary>
        public DeletionResult MayDelete(StationCall call)
        {
            if (!call.IsAtTrainEnd) return new DeletionResult.NotAllowed(call, "CallNotAtTrainEnd");
            var references = ReferencesTo(plan, call);
            return references.Count == 0
                ? new DeletionResult.Success(call)
                : new DeletionResult.Failure(call, references);
        }

        /// <summary>
        /// Deletes a <see cref="StationCall"/> from its train when the rules allow it: removes the call
        /// from its train and its track, and hands its end-of-route role to the neighbour that takes its
        /// place. Returns the refusing result from <c>MayDelete</c> unchanged when the call may not be
        /// deleted, leaving the model untouched.
        /// </summary>
        /// <remarks>
        /// A train's first call departs only and its last call arrives only, and each carries the driver's
        /// service time as its dwell: the origin's arrival is the start of preparing the train, the
        /// terminus's departure the end of finishing it up (see <c>Plan.Create</c>). Deleting an end call
        /// therefore promotes its neighbour to that role and moves the service time across, so the train
        /// keeps a valid shape without the planner having to redo it by hand.
        /// </remarks>
        public DeletionResult TryDelete(StationCall call)
        {
            if (plan.MayDelete(call) is { IsDenied: true } denied) return denied;

            var train = call.Train;
            // Insertion order is not run order; the ends are the earliest and latest call (CallsInRunOrder).
            var ordered = train.CallsInRunOrder;
            var isFirst = ReferenceEquals(call, ordered[0]);
            var serviceMinutes = DwellMinutes(call);

            train.Calls.Remove(call);
            call.Track.Calls.Remove(call);

            if (ordered.Count > 1)
            {
                if (isFirst) MakeOrigin(ordered[1], serviceMinutes);
                else MakeTerminus(ordered[^2], serviceMinutes);
            }
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

    // The minutes a call stands still: a dwell at an intermediate stop, and the driver's preparation or
    // finishing-up time at the train's origin or terminus.
    private static int DwellMinutes(StationCall call) =>
        (int)Math.Round((call.Departure.Value - call.Arrival.Value).TotalMinutes);

    // Makes a call the train's origin: it only departs, and its departure is the anchor, so the
    // preparation time is counted backwards from it.
    private static void MakeOrigin(StationCall call, int preparationMinutes)
    {
        call.Arrival = call.Departure.AddMinutes(-preparationMinutes);
        call.IsArrival = false;
        call.IsDeparture = true;
    }

    // Makes a call the train's terminus: it only arrives, and its arrival is the anchor, so the
    // finishing-up time is counted forwards from it.
    private static void MakeTerminus(StationCall call, int finishingMinutes)
    {
        call.Departure = call.Arrival.AddMinutes(finishingMinutes);
        call.IsArrival = true;
        call.IsDeparture = false;
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
