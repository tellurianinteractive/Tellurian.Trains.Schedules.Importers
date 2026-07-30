using Microsoft.Extensions.Logging;
using System.Data;
using System.Globalization;
using System.Text;
using Tellurian.Trains.Schedules.Importers.Interfaces;
using Tellurian.Trains.Schedules.Importers.Xpln.DataSetProviders;
using Tellurian.Trains.Schedules.Importers.Xpln.Extensions;
using Tellurian.Trains.Schedules.Model;
using static Tellurian.Trains.Schedules.Model.Timetables.TrainExtensions;

namespace Tellurian.Trains.Schedules.Importers.Xpln;

/// <summary>
/// Imports railway schedule data from XPLN spreadsheet files (ODS/XLSX format).
/// This importer reads station tracks, routes, trains, locomotives, trainsets, and driver duties
/// from the XPLN data format and converts them into a complete schedule model.
/// </summary>
/// <remarks>
/// The importer processes three main worksheets from the XPLN file:
/// <list type="bullet">
/// <item><description>StationTrack - Contains station and track definitions</description></item>
/// <item><description>Routes - Contains track stretches between stations</description></item>
/// <item><description>Trains - Contains train definitions, timetables, locomotive and trainset assignments, and driver duties</description></item>
/// </list>
/// </remarks>
public sealed class XplnDataImporter : IImportService, IDisposable
{
    internal record TrainPartKeys(Maybe<StationCall> FromCall, Maybe<StationCall> ToCall, IEnumerable<Message> Messages);

    private readonly Stream _inputStream;
    private readonly IDataSetProvider _dataSetProvider;
    private readonly ILogger _logger;
    private readonly ICompaniesService _operatingCompaniesService;
    private readonly ITrainCategoriesService _trainCategoriesService;
    private readonly XplnImportOptions _options;
    private readonly DataSetConfiguration _dataSetConfiguration = CreateDataSetConfiguration();
    private List<Company> _operatingCompanies = [];
    private List<TrainCategory> _trainCategories = [];
    private DataSet? DataSet;
    private Layout _currentLayout = default!;

    /// <summary>
    /// Initializes a new instance of the <see cref="XplnDataImporter"/> class with a stream input.
    /// </summary>
    /// <param name="inputStream">The input stream containing the XPLN spreadsheet data.</param>
    /// <param name="dataSetProvider">The provider for reading spreadsheet data (ODS or XLSX).</param>
    /// <param name="operatingCompaniesService">Service for retrieving operating company information.</param>
    /// <param name="trainCategoriesService">Service for retrieving train category information.</param>
    /// <param name="logger">Logger for recording import progress and errors.</param>
    /// <param name="options">Per-import language and country settings. Defaults to the current culture.</param>
    public XplnDataImporter(Stream inputStream, IDataSetProvider dataSetProvider, ICompaniesService operatingCompaniesService, ITrainCategoriesService trainCategoriesService, ILogger<XplnDataImporter> logger, XplnImportOptions? options = null)
    {
        _inputStream = inputStream;
        _dataSetProvider = dataSetProvider;
        _operatingCompaniesService = operatingCompaniesService;
        _trainCategoriesService = trainCategoriesService; ;
        _logger = logger;
        _options = options ?? XplnImportOptions.FromCurrentCulture();
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XplnDataImporter"/> class with a file input.
    /// </summary>
    /// <param name="inputFile">The file containing the XPLN spreadsheet data.</param>
    /// <param name="dataSetProvider">The provider for reading spreadsheet data (ODS or XLSX).</param>
    /// <param name="operatingCompaniesService">Service for retrieving operating company information.</param>
    /// <param name="trainCategoriesService">Service for retrieving train category information.</param>
    /// <param name="logger">Logger for recording import progress and errors.</param>
    /// <param name="options">Per-import language and country settings. Defaults to the current culture.</param>
    public XplnDataImporter(FileInfo inputFile, IDataSetProvider dataSetProvider, ICompaniesService operatingCompaniesService, ITrainCategoriesService trainCategoriesService, ILogger<XplnDataImporter> logger, XplnImportOptions? options = null) :
        this(File.OpenRead(inputFile.FullName), dataSetProvider, operatingCompaniesService, trainCategoriesService, logger, options ?? XplnImportOptions.FromFileName(inputFile.Name))
    { }

    private Company? FindOrCreateCompany(string? companySignature)
    {
        if (companySignature.HasValue)
        {
            _ = _operatingCompanies.TryGetFirstValue(oc => oc.Signature.EqualsCaseInsensitive(companySignature), out var company);
            if (company is not null)
            {
                if (!_currentLayout.HasCompany(company))
                {
                    _currentLayout.Add(company);
                }
                return company;
            }
            // Give each discovered company a unique (negative) transient id so trains, vehicles and
            // duties can reference it by CompanyId without colliding; companies found in the catalogue
            // keep their real (positive) ids. Mirrors how categories get NextCategoryId.
            var newCompany = new Company(NextCompanyId, companySignature, companySignature);
            _operatingCompanies.Add(newCompany);
            _currentLayout?.Add(newCompany);
            return newCompany;
        }
        return null;
    }

    private TrainCategory? FindOrCreateCategory(string? trainPrefix, string? backgroundColor)
    {
        _ = _trainCategories.TryGetFirstValue(tc => tc.Prefix.EqualsCaseInsensitive(trainPrefix), out var category);
        if (category is not null) return category;
        if (trainPrefix.HasValue)
        {
            // Give each discovered category a unique (negative) transient id so trains can reference it by
            // CategoryId without colliding; the categories service already uses negative ids by convention.
            var newCategory = new TrainCategory() { Id = NextCategoryId, Name = trainPrefix, Prefix = trainPrefix, Color = backgroundColor ?? "#FFFFFF" };
            _trainCategories.Add(newCategory);
            return newCategory;
        }
        return null;
    }

    private int _nextCategoryId = -3000;
    private int NextCategoryId => Interlocked.Decrement(ref _nextCategoryId);

    private int _nextCompanyId = -2000;
    private int NextCompanyId => Interlocked.Decrement(ref _nextCompanyId);


    /// <summary>
    /// Imports a complete schedule from the XPLN data source.
    /// </summary>
    /// <param name="name">The name to assign to the imported schedule.</param>
    /// <returns>
    /// An <see cref="ImportResult{Schedule}"/> containing the imported schedule if successful,
    /// or validation messages describing any errors encountered during import.
    /// </returns>
    /// <exception cref="IOException">Thrown when the input stream cannot be read.</exception>
    public async Task<ImportResult<Plan>> ImportScheduleAsync(string name)
    {
        _operatingCompanies = [.. await _operatingCompaniesService.GetAllCompaiesAsync()];
        _trainCategories = [.. await _trainCategoriesService.GetAllTrainCategoriesAsync()];
        DataSet = _dataSetProvider.ImportSchedule(_inputStream, _dataSetConfiguration) ?? throw new IOException("Stream cannot be read.");
        return GetImportResult(name);
    }

    private static DataSetConfiguration CreateDataSetConfiguration()
    {
        var result = new DataSetConfiguration("Xpln");
        result.Add(new WorksheetConfiguration("StationTrack", 8));
        result.Add(new WorksheetConfiguration("Routes", 11));
        result.Add(new WorksheetConfiguration("Trains", 18));
        return result;
    }

    private void LogMessages(IEnumerable<Message> messages)
    {
        foreach (var message in messages)
        {
            if (message.Severity == Severity.None) return;
            if (message.Severity == Severity.Error && _logger.IsEnabled(LogLevel.Error)) _logger.LogError("{ErrorMessage}", message.ToString());
            else if (message.Severity == Severity.Warning && _logger.IsEnabled(LogLevel.Warning)) _logger.LogWarning("{WarningMessage}", message.ToString());
            else if (message.Severity == Severity.Information && _logger.IsEnabled(LogLevel.Information)) _logger.LogInformation("{InformationMessage}", message.ToString());
            else if (message.Severity == Severity.System && _logger.IsEnabled(LogLevel.Critical)) _logger.LogCritical("{CriticalMessage}", message.ToString());
        }
    }
    private ImportResult<Plan> GetImportResult(string name)
    {
        var layoutResult = GetLayout(name);
        if (layoutResult.IsFailure)
        {
            var result = new ImportResult<Plan>() { Name = name, Messages = layoutResult.Messages };
            LogMessages(result.Messages);
            return result;
        }
        _currentLayout = layoutResult.Item; // Store layout for company linking
        var timetableResult = GetTimetable(name, layoutResult.Item);
        if (timetableResult.IsFailure)
        {
            var result = new ImportResult<Plan>() { Name = name, Messages = [.. layoutResult.Messages, .. timetableResult.Messages] };
            LogMessages(result.Messages);
            return result;
        }
        // Initialise the layout's operating window from the imported timetable so the graphical
        // timetable axis and the Settings page reflect the real service span (the user can override it).
        (layoutResult.Item.Settings.General.StartTime, layoutResult.Item.Settings.General.EndTime) =
            timetableResult.Item.OperatingWindow();

        // An XPLN file carries no country, so apply the import setting: set the layout's default
        // country and its theme (the user can change them on the Settings page).
        ApplyImportCountry(layoutResult.Item);

        var schedule = GetSchedule(name, timetableResult.Item);
        var ImportResult = schedule with { Name = name, Messages = [.. layoutResult.Messages, .. timetableResult.Messages, .. schedule.Messages] };
        LogMessages(ImportResult.Messages);
        return ImportResult;
    }

    private void ApplyImportCountry(Layout layout)
    {
        var country = Country.ByCountryCode(_options.CountryCode);
        if (country is null) return;
        layout.Settings.Identity.DefaultCountryId = country.Id;
        layout.Settings.Identity.Theme =
            Country.CountriesByTheme(Theme.American).Any(c => c.Id == country.Id) ? Theme.American : Theme.European;
        // Seed the layout's saved country catalogue from the now-known theme, so company/region
        // country references resolve against data that travels with the layout.
        layout.EnsureCountries();
    }

    private ImportResult<Layout> GetLayout(string name)
    {
        var result = new Layout { Name = name };
        var messages = new List<Message>();
        var stations = AddStations(result, messages);
        if (stations.IsFailure) return stations;
        var routes = AddRoutes(result, messages);
        result.DispatchStretches = result.CreateDispatchStretches();
        if (routes.IsFailure) return routes;
        return ImportResult<Layout>.Success(result, messages);
    }

    private ImportResult<Layout> AddStations(Layout layout, List<Message> messages)
    {
        const string WorkSheetName = "StationTrack";
        const int Signature = 0;
        const int TrackName = 2;
        const int Lenght = 3;
        const int Name = 4;
        const int Type = 5;
        const int SubType = 6;
        const int Remark = 7;
        const int MinLength = 7;

        var stations = DataSet?.Tables[WorkSheetName];
        if (stations is null)
            return ImportResult<Layout>.Failure(Message.Error(string.Format(CultureInfo.CurrentCulture, Resources.Strings.WorksheetNotFound, WorkSheetName)));

        messages.Add(Message.Information(string.Format(CultureInfo.CurrentCulture, Resources.Strings.ReadingWorksheet, WorkSheetName)));
        var rowNumber = 1;
        OperationLocation? current = null;
        Station? last = null;
        foreach (DataRow station in stations.Rows)
        {

            if (rowNumber > 1)
            {
                if (IsRepeatedHeader(station)) continue;
                var itemMessages = new List<Message>();
                var fields = station.GetRowFields();
                if (fields.AreAllEmpty) { if (layout.OperationLocations.Count > 0) break; else continue; }
                itemMessages.AddRange(ValidateRow(fields, rowNumber));
                if (itemMessages.HasNoStoppingErrors())
                {
                    if (fields[Type].IsAnyOf("Station,Block"))
                    {
                        if (current is not null)
                        {
                            layout.Add(current);
                            current = null;
                        }
                        var validationMessages = ValidateStation(fields, rowNumber);
                        if (validationMessages.HasNoStoppingErrors())
                        {
                            current = CreateOperationLocation(rowNumber, fields, last);
                            last = current is Station s ? s : null;
                        }
                        itemMessages.AddRange(validationMessages);
                    }
                    else if (fields[Type].IsExpected("Track"))
                    {
                        if (current is null) continue;
                        var validationMessages = ValidateTrack(fields, rowNumber);
                        if (validationMessages.HasNoStoppingErrors())
                        {
                            current.Add(CreateTrack(rowNumber, fields));
                        }
                        itemMessages.AddRange(validationMessages);
                    }
                }
                messages.AddRange(itemMessages);
            }
            rowNumber++;
        }
        if (current is not null) layout.Add(current);

        if (messages.HasStoppingErrors())
            return ImportResult<Layout>.Failure(messages);
        else
            return ImportResult<Layout>.Success(layout, messages);

        static bool IsRepeatedHeader(DataRow row) =>
            row[0].Equals("Name") && row[1].Equals("Enum");

        static OperationLocation CreateOperationLocation(int rowNumber, string[] fields, Station? last)
        {
            return fields[SubType].ToUpperInvariant() switch
            {
                // An XPLN "STATION" is a manned operating place; unmanned places are imported as BLOCK
                // (signal-controlled) or other locations. Marking it manned makes it a dispatch endpoint.
                "STATION" => new Station(rowNumber, fields[Name], fields[Signature]) { IsManned = true },
                "BLOCK" => new SignalControlledLocation(rowNumber, fields[Name], fields[Signature]) { ControlledBy = last },
                _ => new OtherLocation(rowNumber, fields[Name], fields[Signature])
            };
        }

        static StationTrack CreateTrack(int rowNumber, string[] fields) =>
            new(rowNumber, fields[TrackName])
            {
                IsMain = fields[SubType].IsExpected("Main"),
                IsScheduled = fields[SubType].IsAnyOf(["Main", "Depot"]),
                Usage = fields[Remark],
                DisplayOrder = fields[1].NumberOrZero,
            };

        static Message[] ValidateRow(string[] fields, int rowNumber)
        {
            var messages = new List<Message>();
            if (fields.Length < MinLength)
                messages.Add(Message.Error(Resources.Strings.NotAllFieldsArePresent, rowNumber, MinLength, fields.Length));
            if (!fields[Type].OrEmpty.IsAnyOf(["Station", "Track"]))
                messages.Add(Message.Error(Resources.Strings.UnsupportedType, rowNumber, fields[Type]));
            return [.. messages];
        }

        static Message[] ValidateStation(string[] fields, int rowNumber)
        {
            var messages = new List<Message>();
            if (fields[Signature].IsEmpty)
                messages.Add(Message.Error(Resources.Strings.ColumnMustHaveAValue, rowNumber, "Name"));
            if (!fields[SubType].OrEmpty.IsAnyOf(["Station", "Block"]))
                messages.Add(Message.Error(Resources.Strings.UnsupportedSubType, rowNumber, fields[SubType]));
            return [.. messages];
        }

        static Message[] ValidateTrack(string[] fields, int rowNumber)
        {
            var messages = new List<Message>();
            if (fields[TrackName].IsEmpty)
                messages.Add(Message.Error(Resources.Strings.ColumnMustHaveAValue, rowNumber, "TrackName"));
            if (!fields[Lenght].IsEmpty && !fields[Lenght].IsNumber)
                messages.Add(Message.Error(Resources.Strings.ColumnMustBeANumber, rowNumber, "Length", fields[Lenght]));
            if (!fields[SubType].OrEmpty.IsAnyOf(["Main", "Side", "Siding", "Depot", "Goods"]))
                messages.Add(Message.Error(Resources.Strings.UnsupportedSubType, rowNumber, fields[SubType]));
            return [.. messages];
        }
    }

    private ImportResult<Layout> AddRoutes(Layout layout, List<Message> messages)
    {
        const string WorkSheetName = "Routes";
        const string DefaultRoute = "1";
        const int Route = 0;
        const int StartStation = 2;
        const int StartPosition = 3;
        const int EndStation = 4;
        const int EndPosition = 5;
        const int Speed = 6;
        const int Tracks = 7;
        const int Time = 8;

        var routes = DataSet?.Tables[WorkSheetName];
        if (routes is null)
            return ImportResult<Layout>.Failure(Message.Error(string.Format(CultureInfo.CurrentCulture, Resources.Strings.WorksheetNotFound, WorkSheetName)));
        else
            messages.Add(Message.Information(string.Format(CultureInfo.CurrentCulture, Resources.Strings.ReadingWorksheet, WorkSheetName)));

        // Choose the line-grouping strategy for this file. When the Routeid column (A) is shared by
        // two or more rows it identifies the line; otherwise (e.g. Routeid unique per segment, as in
        // some files) the rows form one continuous line as long as each row's start station is the
        // previous row's end station; when that chain breaks, a new timetable stretch begins.
        var groupByRouteId = UsesRouteIdGrouping(routes, Route, StartStation, EndStation);

        var rowNumber = 1;
        TimetableStretch? currentStretch = null;
        var stretchNumber = 0;
        OperationLocation? previousEndStation = null;
        var routeIdStretchOrder = new Dictionary<TimetableStretch, List<(double Position, TrackStretch Stretch)>>();
        foreach (DataRow route in routes.Rows)
        {
            if (rowNumber > 1)
            {
                var itemMessages = new List<Message>();
                var fields = route.GetRowFields();
                if (fields.AreAllEmpty) { if (layout.OperationLocations.Count > 0) break; else continue; }
                if (fields[StartStation].IsZeroesOrEmpty && fields[EndStation].IsZeroesOrEmpty) continue;

                var start = layout.Station(fields[StartStation]);
                var end = layout.Station(fields[EndStation]);
                if (start.IsNone)
                    itemMessages.Add(Message.Error(Resources.Strings.StationNotFoundInLayout, rowNumber, fields[StartStation]));
                if (end.IsNone)
                    itemMessages.Add(Message.Error(Resources.Strings.StationNotFoundInLayout, rowNumber, fields[EndStation]));
                if (!fields[Tracks].IsNumber)
                    itemMessages.Add(Message.Error(Resources.Strings.ColumnMustBeANumber, rowNumber, nameof(Tracks), fields[Tracks]));
                if (!fields[Speed].IsNumber)
                    itemMessages.Add(Message.Error(Resources.Strings.ColumnMustBeANumber, rowNumber, nameof(Speed), fields[Speed]));
                if (!fields[Time].IsNumber)
                    itemMessages.Add(Message.Error(Resources.Strings.ColumnMustBeANumber, rowNumber, nameof(Time), fields[Time]));
                if (!fields[EndPosition].IsNumber)
                    itemMessages.Add(Message.Error(Resources.Strings.ColumnMustBeANumber, rowNumber, nameof(EndPosition), fields[EndPosition]));
                if (itemMessages.HasNoStoppingErrors())
                {
                    if (groupByRouteId)
                    {
                        var routeNumber = fields[Route].HasValue ? fields[Route] : DefaultRoute;
                        if (fields[Route].IsEmpty)
                            itemMessages.Add(Message.Warning(Resources.Strings.RouteNumberIsMissingUsingDefault, rowNumber, routeNumber));
                        if (!layout.HasTimetableStretch(routeNumber))
                        {
                            currentStretch = new TimetableStretch(rowNumber, routeNumber);
                            layout.Add(currentStretch);
                        }
                        else
                        {
                            var ts = layout.TimetableStretch(routeNumber);
                            if (ts.IsNone)
                                itemMessages.Add(Message.Error(Resources.Strings.RouteNotFoundInLayout, rowNumber, routeNumber));
                            else
                                currentStretch = ts.Value;
                        }
                    }
                    else
                    {
                        // A row continues the current line when its start station is the previous row's
                        // end station; otherwise it begins a new line. Continuity is by station, not by the
                        // Position columns: positions may count up or down along a line and can even differ
                        // for the same junction station shared by two lines (e.g. Pa listed at 228.2 then 109.7).
                        var startsNewStretch = currentStretch is null
                            || previousEndStation is null
                            || !previousEndStation.Equals(start.Value);
                        if (startsNewStretch)
                        {
                            stretchNumber++;
                            currentStretch = new TimetableStretch(rowNumber, stretchNumber.ToString(CultureInfo.CurrentCulture));
                            layout.Add(currentStretch);
                        }
                        previousEndStation = end.Value;
                    }
                    if (itemMessages.HasNoStoppingErrors())
                    {
                        // Positions can carry many decimals; the stretch length is only meaningful to ~0.1 km.
                        var distance = Math.Round(Math.Abs(fields[EndPosition].ToDoubleOrZero - fields[StartPosition].ToDoubleOrZero), 1);
                        var stretch = new TrackStretch(rowNumber, start.Value, end.Value, distance, fields[Tracks].ToIntOrZero, fields[Speed].ToIntOrZero, fields[Time].ToIntOrZero);
                        stretch = currentStretch!.AddLast(stretch);
                        layout.Add(stretch);
                        if (groupByRouteId)
                        {
                            if (!routeIdStretchOrder.TryGetValue(currentStretch!, out var order))
                                routeIdStretchOrder[currentStretch!] = order = [];
                            order.Add((fields[StartPosition].ToDoubleOrZero, stretch));
                        }
                    }
                }
                messages.AddRange(itemMessages);
            }
            rowNumber++;
        }
        // When grouping by Routeid, a line's track links may be listed in any row order (e.g.
        // Rotebro2015 line 30 runs Brg(27)->Bgs(30)->Ccw(37) but is listed descending), so order
        // each line's stretches by start position to recover the real station sequence.
        foreach (var (timetableStretch, order) in routeIdStretchOrder)
            timetableStretch.Stretches = [.. order.OrderBy(o => o.Position).Select(o => o.Stretch)];
        if (messages.HasStoppingErrors())
            return ImportResult<Layout>.Failure(messages);
        else
            return ImportResult<Layout>.Success(layout, messages);
    }

    /// <summary>
    /// Determines whether the Routes worksheet uses the Routeid column to group track stretches into lines.
    /// Routeid is a <em>line</em> identifier only when it groups several links per line: there must be more
    /// than one distinct Routeid AND, on average, at least two links per distinct Routeid (Rotebro2015:
    /// 12 links / 3 routes). A single Routeid repeated on every row (Montan all "1") does not distinguish
    /// lines, and Routeid that is essentially unique per link (Givskud2021: 25 links / 24 routes, one stray
    /// duplicate) is a per-segment identifier; both fall back to station-continuity grouping.
    /// </summary>
    private static bool UsesRouteIdGrouping(DataTable routes, int routeColumn, int startStationColumn, int endStationColumn)
    {
        var routeIds = new List<string>();
        var rowNumber = 1;
        foreach (DataRow route in routes.Rows)
        {
            if (rowNumber > 1)
            {
                var fields = route.GetRowFields();
                if (fields.AreAllEmpty) break;
                if (!(fields[startStationColumn].IsZeroesOrEmpty && fields[endStationColumn].IsZeroesOrEmpty)
                    && fields[routeColumn].HasValue)
                    routeIds.Add(fields[routeColumn]);
            }
            rowNumber++;
        }
        var distinct = new HashSet<string>(routeIds, StringComparer.OrdinalIgnoreCase).Count;
        return distinct >= 2 && routeIds.Count >= 2 * distinct;
    }

    /// <summary>
    /// Resolves the final type of XPLN "trainset" entries.
    /// A trainset that is listed under both the locomotive and the trainset section with the same
    /// identifier is a self-propelled railcar (e.g. the Swedish X2000): the pair represents one
    /// physical vehicle and is merged into a single <see cref="ScheduledObjectType.Trainset"/>.
    /// Any remaining trainset is not self-propelled; per XPLN semantics it is reclassified as a
    /// <see cref="ScheduledObjectType.Wagonset"/>.
    /// </summary>
    private static void MergeRailcarsAndReclassifyTrainsets(Plan schedule)
    {
        var railcarGroups = schedule.ScheduledObjects
            .Where(v => v.ExternalId.HasValue && (v.ObjectType == ScheduledObjectType.Locomotive || v.ObjectType == ScheduledObjectType.Trainset))
            .GroupBy(v => v.ExternalId!, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Any(v => v.ObjectType == ScheduledObjectType.Locomotive) && g.Any(v => v.ObjectType == ScheduledObjectType.Trainset))
            .ToList();

        var mergedRailcarIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var group in railcarGroups)
        {
            var members = group.ToList();
            var railcar = members[0];
            railcar.ObjectType = ScheduledObjectType.Trainset;
            mergedRailcarIds.Add(group.Key);
            var primarySchedule = railcar.ScheduleAssignments.Select(a => a.Schedule).FirstOrDefault();

            foreach (var duplicate in members.Skip(1))
            {
                foreach (var assignment in duplicate.ScheduleAssignments)
                {
                    var duplicateSchedule = assignment.Schedule;
                    if (primarySchedule is null || duplicateSchedule is null || duplicateSchedule.Id == primarySchedule.Id) continue;
                    foreach (var part in duplicateSchedule.Parts.ToList())
                        if (!primarySchedule.Parts.Contains(part)) primarySchedule.Add(part);
                    schedule.Schedules.Remove(duplicateSchedule);
                }
                schedule.ScheduledObjects.Remove(duplicate);
            }
        }

        // A trainset that is not paired with a locomotive of the same identifier is not a
        // self-propelled railcar; it is a wagon set. (Cargo flows are not yet distinguished here.)
        foreach (var vehicle in schedule.ScheduledObjects)
            if (vehicle.ObjectType == ScheduledObjectType.Trainset &&
                (vehicle.ExternalId.IsEmpty || !mergedRailcarIds.Contains(vehicle.ExternalId!)))
                vehicle.ObjectType = ScheduledObjectType.Wagonset;
    }

    // XPLN convention: a vehicle schedule that works a single train, where that train is also the only
    // train in a duty, is a special "driver's choice" working that runs on demand rather than on fixed
    // sessions. Mark such trains on demand so the schedule/turnus rules treat them as unsessioned (in
    // particular they need not close the loop back to their starting station).
    private static void MarkSingleTrainWorkingsOnDemand(Plan schedule)
    {
        var onDemand = Sessions.FromBitPattern(CommonSessionPatterns.OnDemand);
        foreach (var vehicleSchedule in schedule.Schedules)
        {
            var trains = vehicleSchedule.Parts.Select(p => p.Train).Distinct().ToList();
            if (trains.Count != 1) continue;
            var train = trains[0];
            var isOnlyTrainInADuty = schedule.DriverDuties.Any(duty =>
            {
                var dutyTrains = duty.Parts.Select(p => p.Train).Distinct().ToList();
                return dutyTrains.Count == 1 && dutyTrains[0].Equals(train);
            });
            if (isOnlyTrainInADuty) train.Sessions = onDemand;
        }
    }

    // Links each imported Job segment to the existing traction schedule part(s) that cover it, so a driver
    // duty references the same shared ScheduledTrainPart instances the vehicle schedules own (which is how
    // a part's traction unit is resolved). For each segment, every traction part of the same train whose
    // span lies within the segment is added to the duty — a segment spanning two traction parts (a loco
    // change mid-Job) adds both, which is what produces the traction-exchange note. When no traction part
    // matches (e.g. a deadhead/walk segment), a standalone part is added so no Job data is lost.
    private static void LinkJobSegmentsToTractionParts(Plan plan, List<(DriverDuty Duty, StationCall From, StationCall To)> segments)
    {
        foreach (var (duty, from, to) in segments)
        {
            var matches = plan.Schedules
                .SelectMany(s => s.Parts)
                .Where(p => p.Train.Equals(from.Train)
                    && p.From.Departure >= from.Departure
                    && p.To.Arrival <= to.Arrival
                    && plan.ScheduledObjectsFor(p).Any(so => so.IsTraction))
                .OrderBy(p => p.From.Departure)
                .ToList();
            if (matches.Count > 0)
                foreach (var part in matches) duty.Add(part);
            else
                duty.Add(new ScheduledTrainPart(from, to));
        }
    }

    private ImportResult<Timetable> GetTimetable(string name, Layout layout)
    {
        const string WorkSheetNameAndObjects = "Trains:traindef,timetable,remarks";
        const string WorkSheetName = "Trains";
        const int Number = 0;
        const int Station = 2;
        const int Track = 3;
        const int Arrival = 4;
        const int Departure = 5;
        const int Wheel = 6;
        const int Group = 6;
        const int Object = 7;
        const int Type = 8;
        const int Remark = 10;
        const int MinLength = 10;

        List<Message> messages = [];

        var trains = DataSet?.Tables[WorkSheetName];
        if (trains is null)
        {
            messages.Add(Message.System(string.Format(CultureInfo.CurrentCulture, Resources.Strings.WorksheetNotFound, WorkSheetName)));
            return ImportResult<Timetable>.Failure(messages);
        }

        messages.Add(Message.Information(string.Format(CultureInfo.CurrentCulture, Resources.Strings.ReadingWorksheet, WorkSheetNameAndObjects)));
        var result = new Timetable(name, layout);
        var rowNumber = 1;
        var callNumber = 0;
        Train? current = null;

        foreach (DataRow row in trains.Rows)
        {
            if (rowNumber > 1)
            {
                var itemMessages = new List<Message>();
                var fields = row.GetRowFields();
                if (fields.AreAllEmpty) { if (result.Trains.Count > 0) break; else continue; }
                itemMessages.AddRange(ValidateRow(fields, rowNumber));
                if (itemMessages.HasNoStoppingErrors())
                {
                    var type = fields[Type].ToLowerInvariant();
                    switch (type)
                    {
                        case "traindef":
                            {
                                if (current is not null)
                                {
                                    messages.AddRange(AddTrain(result, current, rowNumber));
                                    current = null;
                                    callNumber = 0;
                                }

                                var validationMessages = ValidateTrain(fields, rowNumber);
                                if (validationMessages.HasNoStoppingErrors())
                                {
                                    var trainCategoryPrefix = fields[Object].TrainCategoryPrefix;
                                    var category = FindOrCreateCategory(trainCategoryPrefix, row.BackgroundColor(_dataSetConfiguration.BackgroundColorColumIndex(WorkSheetName)));
                                    current = CreateTrain(rowNumber, fields, category);
                                }
                                messages.AddRange(validationMessages);
                            }
                            break;

                        case "timetable":
                            {
                                if (current is null) continue;
                                var validationMessages = ValidateCall(fields, rowNumber);
                                if (validationMessages.HasNoStoppingErrors())
                                {
                                    callNumber++;
                                    var track = layout.Track(fields[Station], fields[Track]);
                                    if (track.IsNone)
                                    {
                                        messages.Add(Message.Error(string.Format(CultureInfo.CurrentCulture, Resources.Strings.RowMessage, rowNumber, track.Message)));
                                    }
                                    else
                                    {
                                        current.Add(CreateCall(rowNumber, fields, track.Value));
                                    }
                                }
                                messages.AddRange(validationMessages);
                            }
                            break;

                        case "locomotive":
                            {
                                if (current.IsNullOrHasNoCalls()) continue;

                                if (fields[Object].HasValue)
                                {
                                    var note = new TextCallNote(fields[Remark].HasValue ?
                                            string.Format(CultureInfo.CurrentCulture, Resources.Strings.UseLocoClasses, fields[Object], fields[Remark]) :
                                            string.Format(CultureInfo.CurrentCulture, Resources.Strings.UseLoco, fields[Object]),
                                            _options.Language)
                                    {
                                        IsDriverNote = true,
                                        IsStationNote = true,
                                        IsForDeparture = true,
                                    };
                                    var train = result.Trains.SingleOrDefault(t => t.Equals(current));
                                    if (train is not null)
                                    {
                                        train.Calls.First().Notes.Add(note);
                                        var companySignature = fields[Object].LocoOperatingCompanySignature;
                                        train.Company = FindOrCreateCompany(companySignature);
                                    }
                                }
                            }
                            break;

                        case "trainset":
                            {
                                if (current.IsNullOrHasNoCalls()) continue;
                                if (fields[Remark].HasValue)
                                {
                                    var note =
                                        new TextCallNote($"{fields[Group]}: {fields[Object].WithQuotationMarksRemoved} {fields[Remark].WithQuotationMarksRemoved}", _options.Language)
                                        {
                                            IsDriverNote = true,
                                            IsForDeparture = true,
                                        };
                                    current?.Calls.First().Notes.Add(note);
                                }

                            }
                            break;
                        case "wheel":
                            {
                                if (current is null) break;
                                if (int.TryParse(fields[Wheel], out var axles) && axles > 0)
                                {
                                    current.Length = TrainCapacity.AxlesOnly(axles);
                                }
                            }
                            break;
                        case "group":
                            if (current?.Category is null) break;
                            // XPLN's group row classifies the train as freight (G_Zug) or passenger
                            // (P_Zug); apply that to the train's category. Other group values carry no
                            // freight/passenger meaning and are ignored.
                            switch (fields[Object])
                            {
                                case "G_Zug": current.Category.IsFreight = true; current.Category.IsPassenger = false; break;
                                case "P_Zug": current.Category.IsPassenger = true; current.Category.IsFreight = false; break;
                            }
                            break;
                    }
                }
                messages.AddRange(itemMessages);
            }
            rowNumber++;
        }
        if (current is not null) messages.AddRange(AddTrain(result, current, rowNumber));
        if (messages.HasStoppingErrors())
            return ImportResult<Timetable>.Failure(messages);

        // Populate the timetable's category catalogue with exactly the categories the imported trains use
        // (mirrors how only referenced companies are added to the layout). The colour each train is drawn
        // with rides on Train.Category; this makes the same categories available on the Train Categories tab.
        result.TrainCategories = [.. result.Trains.Select(t => t.Category).OfType<TrainCategory>().Distinct()];
        return ImportResult<Timetable>.Success(result, messages);

        static IEnumerable<Message> AddTrain(Timetable timetable, Train train, int rowNumber)
        {
            if (train.Calls.Count == 0)
            {
                return [Message.Error(string.Format(CultureInfo.CurrentCulture, Resources.Strings.TrainHasNoCalls, rowNumber, train))];
            }
            else
            {
                timetable.Add(train
                    .WithFixedSingleCallTrain()
                    .WithOriginAndTerminusDwell()
                    .WithFirstCallDepartureOnlyAndLastCallArrivalOnly()
                    .WithPassthroughCalls());
                return [];
            }

        }

        static Train CreateTrain(int rowNumber, string[] fields, TrainCategory? category)
        {
            var train = new Train(rowNumber, ExtractTrainNumber(rowNumber, fields), fields[Object])
            {
                Remark = fields[Remark],
                Category = category,
                CategoryId = category?.Id
            };
            return train;
        }

        static int ExtractTrainNumber(int rowNumber, string[] fields)
        {
            var A = fields[Number];
            var result = A.ToIntOrZero;
            if (result > 0) return result;
            // if chars after a space in A is only digits, its a train number ("RB62 75509" -> 75509)
            var afterSpace = A[(A.LastIndexOf(' ') + 1)..];
            if (afterSpace.Length > 0)
            {
                result = afterSpace.ToIntOrZero;
                if (result > 0) return result;
            }
            // if A contains a single non-broken sequence of digits in any position, its a train number ("RB6201" -> 6201)
            if (SingleDigitRun(A) is { } singleRun)
            {
                result = singleRun.ToIntOrZero;
                if (result > 0) return result;
            }
            return rowNumber;

            // Returns the one unbroken run of digits in value, or null when there is none or more than one.
            static string? SingleDigitRun(string value)
            {
                string? run = null;
                var start = -1;
                for (var i = 0; i <= value.Length; i++)
                {
                    var isDigit = i < value.Length && char.IsDigit(value[i]);
                    if (isDigit && start < 0) start = i;
                    else if (!isDigit && start >= 0)
                    {
                        if (run is not null) return null; // a second run: not a single sequence
                        run = value[start..i];
                        start = -1;
                    }
                }
                return run;
            }
        }

        // Every call is created as a full stop (arrives and departs). The stop flags are then refined per the
        // XPLN conventions once the whole train is known: the origin (first call) is always a departure and the
        // terminus (last call) always an arrival, so when their times are equal a 10-minute dwell is synthesised
        // (WithOriginAndTerminusDwell) to keep them as stops; the first call is then made departure only and the
        // last call arrival only (WithFirstCallDepartureOnlyAndLastCallArrivalOnly); and any intermediate call
        // whose arrival equals its departure becomes a pass-through with no stop (WithPassthroughCalls).
        static StationCall CreateCall(int rowNumber, string[] fields, StationTrack track)
        {
            return new(rowNumber, track, fields[Arrival].AsTime(), fields[Departure].AsTime(), fields[Remark])
            {
                IsArrival = true,
                IsDeparture = true,
            };
        }

        static Message[] ValidateRow(string[] fields, int rowNumber)
        {
            var messages = new List<Message>();
            if (fields.Length < MinLength)
                messages.Add(Message.Error(string.Format(CultureInfo.CurrentCulture, Resources.Strings.NotAllFieldsArePresent, rowNumber, MinLength, fields.Length)));
            if (!fields[Arrival].IsTime())
                messages.Add(Message.Error(string.Format(CultureInfo.CurrentCulture, Resources.Strings.ColumnMustBeATime, rowNumber, "Arrival", fields[Arrival])));
            if (!fields[Departure].IsTime())
                messages.Add(Message.Error(string.Format(CultureInfo.CurrentCulture, Resources.Strings.ColumnMustBeATime, rowNumber, "Departure", fields[Departure])));
            else if (!fields[Type].IsAnyOf(["Traindef", "Timetable", "Locomotive", "Trainset", "Job", "Wheel", "Group"]))
                messages.Add(Message.Error(string.Format(CultureInfo.CurrentCulture, Resources.Strings.UnsupportedType, rowNumber, fields[Type])));
            return [.. messages];
        }

        static Message[] ValidateTrain(string[] fields, int rowNumber)
        {
            var messages = new List<Message>();
            if (fields[Object].IsEmpty)
                messages.Add(Message.Error(string.Format(CultureInfo.CurrentCulture, Resources.Strings.ColumnMustHaveAValue, rowNumber, "Object")));
            return [.. messages];
        }

        static Message[] ValidateCall(string[] fields, int rowNumber)
        {
            var messages = new List<Message>();
            if (fields[Track].IsEmpty)
                messages.Add(Message.Error(string.Format(CultureInfo.CurrentCulture, Resources.Strings.ColumnMustHaveAValue, rowNumber, "Track")));
            return [.. messages];
        }
    }

    private ImportResult<Plan> GetSchedule(string name, Timetable timetable)
    {
        const string WorkSheetNameAndObjects = "Trains:locomotive,trainset,job,remarks";
        const string WorkSheetName = "Trains";
        const int TrainNumber = 0;
        const int From = 2;
        const int To = 3;
        const int Arrival = 4;
        const int Departure = 5;
        const int Object = 7;
        const int Type = 8;
        const int TrainName = 9;
        const int MinLength = 9;
        const int Remark = 10;

        var messages = new List<Message>();
        var locoSchedules = new Dictionary<string, Schedule>(100);
        var trainsetSchedules = new Dictionary<string, Schedule>(200);
        var driverDuties = new Dictionary<string, DriverDuty>();
        // Job rows are resolved to the existing traction schedule parts in a post-pass (see
        // LinkJobSegmentsToTractionParts), once every loco/trainset schedule has been built. Each entry is
        // the duty a Job row belongs to and the segment (from/to calls) it covers on its train.
        var jobSegments = new List<(DriverDuty Duty, StationCall From, StationCall To)>();

        var trains = DataSet?.Tables[WorkSheetName];
        if (trains is null)
        {
            messages.Add(Message.System(string.Format(CultureInfo.CurrentCulture, Resources.Strings.WorksheetNotFound, WorkSheetName)));
            return ImportResult<Plan>.Failure(messages);
        }

        messages.Add(Message.Information(string.Format(CultureInfo.CurrentCulture, Resources.Strings.ReadingWorksheet, WorkSheetNameAndObjects)));
        var schedule = Plan.Create(name, timetable);
        Train? currentTrain = null;

        var rowNumber = 1;
        foreach (DataRow row in trains.Rows)
        {
            if (rowNumber > 1)
            {
                var itemMessages = new List<Message>();
                var fields = row.GetRowFields();
                if (fields.AreAllEmpty) { if (locoSchedules.Count > 0 || trainsetSchedules.Count > 0 || driverDuties.Count > 0) break; else continue; }
                itemMessages.AddRange(ValidateRow(fields, rowNumber));
                if (itemMessages.HasNoStoppingErrors())
                {
                    var type = fields[Type].ToLowerInvariant();
                    switch (type)
                    {
                        case "traindef":
                            {
                                var trainExternalId = fields[Object];
                                var train = schedule.Timetable.Train(trainExternalId);
                                if (train.IsNone)
                                {
                                    messages.Add(Message.Error(train.Message));
                                    currentTrain = null;
                                    break;
                                }
                                currentTrain = train.Value;
                            }
                            break;
                        case "locomotive":
                            {
                                if (currentTrain is null) break;

                                var locoMessages = new List<Message>();
                                locoMessages.AddRange(ValidateLoco(fields, rowNumber));

                                if (locoMessages.HasNoStoppingErrors())
                                {
                                    var locoId = fields[Object];
                                    if (!locoSchedules.ContainsKey(locoId))
                                    {
                                        var operatorSignature = fields[Object].LocoOperatingCompanySignature;
                                        var company = FindOrCreateCompany(operatorSignature);
                                        var vehicleSchedule = schedule.CreateVehicleWithAllSessionsSchedule(
                                            id: rowNumber,
                                            vehicleType: ScheduledObjectType.Locomotive,
                                            number: locoId.LocoNumber,
                                            company: company,
                                            externalId: locoId);
                                        locoSchedules.Add(locoId, vehicleSchedule);
                                    }
                                    if (locoSchedules.TryGetValue(locoId, out var loco))
                                    {
                                        var keys = GetTrainPartKeys(fields, currentTrain, rowNumber);
                                        locoMessages.AddRange(keys.Messages);
                                        if (locoMessages.HasNoStoppingErrors())
                                        {
                                            ScheduledTrainPart trainPart = new ScheduledTrainPart(keys.FromCall.Value, keys.ToCall.Value)
                                            {
                                                TractionOptions = new TractionOptions()
                                            };
                                            loco.Add(trainPart);
                                        }
                                    }
                                }
                                messages.AddRange(locoMessages);
                            }
                            break;
                        case "trainset":
                            {
                                if (currentTrain is null) break;
                                var trainsetMessages = new List<Message>();
                                trainsetMessages.AddRange(ValidateTrainset(fields, rowNumber));
                                if (trainsetMessages.HasNoStoppingErrors())
                                {
                                    var trainsetId = fields[Object].WithQuotationMarksRemoved;
                                    if (trainsetId.HasValue) // This is a trainset with schedule
                                    {
                                        if (!trainsetSchedules.ContainsKey(trainsetId))
                                        {
                                            var vehicleSchedule = schedule.CreateVehicleWithAllSessionsSchedule(
                                                id: rowNumber,
                                                vehicleType: ScheduledObjectType.Trainset,
                                                number: trainsetId.LocoNumber,
                                                externalId: trainsetId,
                                                // Use the actual remark column, not the identifier. When it merely
                                                // repeats the external id, the turnus card suppresses it (see TurnusData).
                                                remark: fields[Remark].WithQuotationMarksRemoved);
                                            trainsetSchedules.Add(trainsetId, vehicleSchedule);
                                        }
                                        if (trainsetSchedules.TryGetValue(trainsetId, out var trainset))
                                        {
                                            var keys = GetTrainPartKeys(fields, currentTrain, rowNumber);
                                            trainsetMessages.AddRange(keys.Messages);
                                            if (trainsetMessages.HasNoStoppingErrors())
                                            {
                                                ScheduledTrainPart trainPart = new ScheduledTrainPart(keys.FromCall.Value, keys.ToCall.Value)
                                                {
                                                    WagonSetOptions = new WagonSetOptions()
                                                };
                                                trainset.Add(trainPart);
                                            }
                                        }
                                    }
                                    else // A 'trainset' row with no id but a remark is a cargo flow:
                                         // freight wagons (directed by waybills) that the train couples and later uncouples.
                                    {
                                        if (fields[Object].IsEmpty && fields[Remark].IsEmpty) continue; // No information about the cargo flow, despite a row in the data.
                                        var keys = GetTrainPartKeys(fields, currentTrain, rowNumber);
                                        trainsetMessages.AddRange(keys.Messages);
                                        if (trainsetMessages.HasNoStoppingErrors())
                                        {
                                            // A cargo flow carries no XPLN identifier; synthesise a unique one so each flow
                                            // is a distinct Cargo object (identity is ObjectType + ExternalId).
                                            var cargoFlow = schedule.CreateVehicleWithAllSessionsSchedule(
                                                id: rowNumber,
                                                vehicleType: ScheduledObjectType.CargoFlow,
                                                number: 0,
                                                externalId: $"WagonGroup{rowNumber}",
                                                remark: fields[Remark]);
                                            // The imported cargo flow is a ScheduledObject(CargoFlow) assigned to its
                                            // own schedule; the bare part records the segment it runs over. Structured
                                            // routing (CargoFlowOptions) is added later through the cargo-flow editor.
                                            ScheduledTrainPart trainPart = new ScheduledTrainPart(keys.FromCall.Value, keys.ToCall.Value);
                                            cargoFlow.Add(trainPart);
                                        }
                                    }
                                }
                                messages.AddRange(trainsetMessages);
                            }
                            break;
                        case "job":
                            {
                                if (currentTrain is null) break;
                                var dutyMessages = new List<Message>();
                                dutyMessages.AddRange(ValidateJob(fields, rowNumber));
                                if (dutyMessages.HasNoStoppingErrors())
                                {
                                    var jobId = fields[Object].OrElse(fields[TrainNumber]);
                                    if (!driverDuties.ContainsKey(jobId))
                                        driverDuties.Add(jobId, new DriverDuty(rowNumber, jobId) { });
                                    if (driverDuties.TryGetValue(jobId, out var duty))
                                    {
                                        var keys = GetTrainPartKeys(fields, currentTrain, rowNumber);
                                        dutyMessages.AddRange(keys.Messages);
                                        if (dutyMessages.HasNoStoppingErrors())
                                        {
                                            // Defer to the post-pass: link this segment to the existing traction
                                            // schedule part(s) rather than minting a standalone part here.
                                            jobSegments.Add((duty, keys.FromCall.Value, keys.ToCall.Value));
                                        }
                                    }

                                }
                                messages.AddRange(dutyMessages);
                            }
                            break;
                    }
                }

            }
            rowNumber++;
        }
        if (messages.HasStoppingErrors()) return ImportResult<Plan>.Failure(messages);
        // Vehicles and VehicleSchedules are already added by CreateVehicleWithAllSessionsSchedule
        foreach (var duty in driverDuties.Values) schedule.AddDriverDuty(duty);
        MergeRailcarsAndReclassifyTrainsets(schedule);
        // With every traction schedule built and reclassified, link each imported Job segment to the
        // traction schedule part(s) it is driven by, so a duty references the same shared parts as the
        // schedules (before the on-demand pass, which reads the duties' parts).
        LinkJobSegmentsToTractionParts(schedule, jobSegments);
        MarkSingleTrainWorkingsOnDemand(schedule);
        return ImportResult<Plan>.Success(schedule, messages);

        static TrainPartKeys GetTrainPartKeys(string[] fields, Train currentTrain, int rowNumber)
        {
            var messages = new List<Message>();
            var (start, end, startTime, endTime) = GetTrainPartFields(fields);
            if (startTime > endTime)
                messages.Add(Message.Error(string.Format(CultureInfo.CurrentCulture, Resources.Strings.ObjectInTrainHasWrongTimingEndStartionIsBeforeStartStation, rowNumber, ObjectDescription(fields), currentTrain, fields[Arrival].AsTime(), fields[Departure].AsTime())));
            var (fromCall, fromIndex) = currentTrain.FindBetweenArrivalAndDeparture(start, startTime, rowNumber);
            var (toCall, toIndex) = currentTrain.FindBetweenArrivalAndDeparture(end, endTime, rowNumber);
            if (messages.HasNoStoppingErrors())
            {
                if (fromCall.IsNone)
                {
                    messages.Add(Message.Error(fromCall.Message));
                    messages.Add(Message.Error(string.Format(CultureInfo.CurrentCulture, Resources.Strings.ObjectAtStationWithDepartureDoNotRefersToAnExistingTimeInTrain, rowNumber, ObjectDescription(fields), fields[From], fields[Departure].AsTime(), currentTrain)));
                }
                if (toCall.IsNone)
                {
                    messages.Add(Message.Error(toCall.Message));
                    messages.Add(Message.Error(string.Format(CultureInfo.CurrentCulture, Resources.Strings.ObjectAtStationWithArrivalDoNotRefersToAnExistingTimeInTrain, rowNumber, ObjectDescription(fields), fields[To], fields[Arrival].AsTime(), currentTrain)));
                }
                if (toCall.HasValue && fromCall.HasValue && fromIndex >= toIndex)
                    messages.Add(Message.Error(string.Format(CultureInfo.CurrentCulture, Resources.Strings.ObjectInTrainHasWrongTimingEndStartionIsBeforeStartStation, rowNumber, ObjectDescription(fields), currentTrain, fields[Departure].AsTime(), fields[Arrival].AsTime())));
            }
            return new TrainPartKeys(fromCall, toCall, messages);
        }

        static string ObjectDescription(string[] fields) => fields[Object].HasValue ? $"{fields[Type]}:{fields[Object]}".Trim() : fields[Type];


        static (string from, string to, Time departure, Time arrival) GetTrainPartFields(string[] fields) =>
           (fields[From], fields[To], fields[Arrival].AsTime(), fields[Departure].AsTime());

        static Message[] ValidateRow(string[] fields, int rowNumber)
        {
            var messages = new List<Message>();
            if (fields.Length < MinLength)
                messages.Add(Message.Error(string.Format(CultureInfo.CurrentCulture, Resources.Strings.NotAllFieldsArePresent, rowNumber, MinLength, fields.Length)));
            if (!fields[Arrival].IsTime(fields[Type] == "timetable"))
                messages.Add(Message.Error(string.Format(CultureInfo.CurrentCulture, Resources.Strings.ColumnMustBeATime, rowNumber, "Arrival", fields[Arrival])));
            if (!fields[Departure].IsTime(fields[Type] == "timetable"))
                messages.Add(Message.Error(string.Format(CultureInfo.CurrentCulture, Resources.Strings.ColumnMustBeATime, rowNumber, "Departure", fields[Arrival])));
            else if (!fields[Type].IsAnyOf(["Traindef", "Timetable", "Locomotive", "Trainset", "Job", "Wheel", "Group"]))
                messages.Add(Message.Error(string.Format(CultureInfo.CurrentCulture, Resources.Strings.UnsupportedType, rowNumber, fields[Type])));
            return [.. messages];
        }

        static Message[] ValidateLoco(string[] fields, int rowNumber)
        {
            var messages = new List<Message>();
            if (fields[Object].IsEmpty)
                messages.Add(Message.Error(string.Format(CultureInfo.CurrentCulture, Resources.Strings.ColumnMustHaveAValue, rowNumber, "Object")));
            return [.. messages];
        }
        static Message[] ValidateJob(string[] fields, int rowNumber)
        {
            var messages = new List<Message>();
            if (fields[Object].OrElse(fields[TrainNumber]).IsEmpty)
                messages.Add(Message.Error(string.Format(CultureInfo.CurrentCulture, Resources.Strings.ColumnMustHaveAValue, rowNumber, "Object|TrainNumber")));
            return [.. messages];
        }
        static Message[] ValidateTrainset(string[] fields, int rowNumber)
        {
            var messages = new List<Message>();
            if (fields[Object].IsEmpty && fields[TrainName].IsEmpty)
                messages.Add(Message.Error(string.Format(CultureInfo.CurrentCulture, Resources.Strings.ColumnMustHaveAValue, rowNumber, "Object|TrainName")));
            return [.. messages];
        }
    }

    #region IDisposable

    private bool IsDisposed;
    private void Dispose(bool disposing)
    {
        if (!IsDisposed)
        {
            if (disposing)
            {

                DataSet?.Dispose();
                if (_dataSetProvider is IDisposable disposable) disposable.Dispose();
                _inputStream?.Dispose();
            }
            IsDisposed = true;
        }
    }

    /// <summary>
    /// Releases all resources used by the <see cref="XplnDataImporter"/>.
    /// </summary>
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    #endregion
}
