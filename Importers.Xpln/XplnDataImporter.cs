using Microsoft.Extensions.Logging;
using System.Data;
using System.Globalization;
using System.Text;
using Tellurian.Trains.Schedules.Importers.Interfaces;
using Tellurian.Trains.Schedules.Importers.Services;
using Tellurian.Trains.Schedules.Importers.Xpln.DataSetProviders;
using Tellurian.Trains.Schedules.Importers.Xpln.Extensions;
using Tellurian.Trains.Schedules.Model;
using static Tellurian.Trains.Schedules.Model.TrainExtensions;

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
    public XplnDataImporter(Stream inputStream, IDataSetProvider dataSetProvider, ICompaniesService operatingCompaniesService, ITrainCategoriesService trainCategoriesService, ILogger<XplnDataImporter> logger)
    {
        _inputStream = inputStream;
        _dataSetProvider = dataSetProvider;
        _operatingCompaniesService = operatingCompaniesService;
        _trainCategoriesService = trainCategoriesService; ;
        _logger = logger;
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
    public XplnDataImporter(FileInfo inputFile, IDataSetProvider dataSetProvider, ICompaniesService operatingCompaniesService, ITrainCategoriesService trainCategoriesService, ILogger<XplnDataImporter> logger) :
        this(File.OpenRead(inputFile.FullName), dataSetProvider, operatingCompaniesService, trainCategoriesService, logger)
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
            var newCompany = Company.FromSignature(companySignature);
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
            var newCategory = new TrainCategory() { ResourceName = trainPrefix, Prefix = trainPrefix, Color = backgroundColor ?? "#FFFFFF" };
            _trainCategories.Add(newCategory);
            return newCategory;
        }
        return null;
    }


    /// <summary>
    /// Imports a complete schedule from the XPLN data source.
    /// </summary>
    /// <param name="name">The name to assign to the imported schedule.</param>
    /// <returns>
    /// An <see cref="ImportResult{Schedule}"/> containing the imported schedule if successful,
    /// or validation messages describing any errors encountered during import.
    /// </returns>
    /// <exception cref="IOException">Thrown when the input stream cannot be read.</exception>
    public async Task<ImportResult<Schedule>> ImportScheduleAsync(string name)
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
    private ImportResult<Schedule> GetImportResult(string name)
    {
        var layoutResult = GetLayout(name);
        if (layoutResult.IsFailure)
        {
            var result = new ImportResult<Schedule>() { Name = name, Messages = layoutResult.Messages };
            LogMessages(result.Messages);
            return result;
        }
        _currentLayout = layoutResult.Item; // Store layout for company linking
        var timetableResult = GetTimetable(name, layoutResult.Item);
        if (timetableResult.IsFailure)
        {
            var result = new ImportResult<Schedule>() { Name = name, Messages = [.. layoutResult.Messages, .. timetableResult.Messages] };
            LogMessages(result.Messages);
            return result;
        }
        var schedule = GetSchedule(name, timetableResult.Item);
        var ImportResult = schedule with { Name = name, Messages = [.. layoutResult.Messages, .. timetableResult.Messages, .. schedule.Messages] };
        LogMessages(ImportResult.Messages);
        return ImportResult;
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
                "STATION" => new Station(rowNumber, fields[Name], fields[Signature]),
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

        var rowNumber = 1;
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
                    TimetableStretch? timetableStretch = null;
                    var routeNumber = fields[Route].HasValue ? fields[Route] : DefaultRoute;
                    if (fields[Route].IsEmpty)
                    {
                        itemMessages.Add(Message.Warning(Resources.Strings.RouteNumberIsMissingUsingDefault, rowNumber, routeNumber));
                    }
                    if (!layout.HasTimetableStretch(routeNumber))
                    {
                        timetableStretch = new TimetableStretch(rowNumber, routeNumber);
                        layout.Add(timetableStretch);
                    }
                    else
                    {
                        var ts = layout.TimetableStretch(routeNumber);
                        if (ts.IsNone)
                        {
                            itemMessages.Add(Message.Error(Resources.Strings.RouteNotFoundInLayout, rowNumber, routeNumber));
                        }
                        else
                        {
                            timetableStretch = ts.Value;
                        }
                    }
                    if (itemMessages.HasNoStoppingErrors())
                    {
                        var distance = Math.Abs(fields[EndPosition].ToDoubleOrZero - fields[StartPosition].ToDoubleOrZero);
                        var stretch = new TrackStretch(rowNumber, start.Value, end.Value, distance, fields[Tracks].ToIntOrZero, fields[Speed].ToIntOrZero, fields[Time].ToIntOrZero);
                        stretch = timetableStretch!.AddLast(stretch);
                        layout.Add(stretch);
                    }
                }
                messages.AddRange(itemMessages);
            }
            rowNumber++;
        }
        if (messages.HasStoppingErrors())
            return ImportResult<Layout>.Failure(messages);
        else
            return ImportResult<Layout>.Success(layout, messages);
    }

    private ImportResult<Timetable> GetTimetable(string name, Layout layout)
    {
        const string WorkSheetNameAndObjects = "Trains:traindef,timetable,remarks";
        const string WorkSheetName = "Trains";
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
                                            string.Format(CultureInfo.CurrentCulture, Resources.Strings.UseLoco, fields[Object]))
                                    {
                                        IsDriverNote = true,
                                        IsStationNote = true,
                                        IsForDeparture = true,
                                        LanguageCode = CultureInfo.CurrentCulture.TwoLetterISOLanguageName
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
                                    var note = new TextCallNote($"{fields[Group]}: {fields[Object].WithQuotationMarksRemoved} {fields[Remark].WithQuotationMarksRemoved}")
                                    {
                                        IsDriverNote = true,
                                        IsForDeparture = true,
                                        LanguageCode = CultureInfo.CurrentCulture.TwoLetterISOLanguageName
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
                                    current.Length = TrainLenght.AxlesOnly(axles);
                                }
                            }
                            break;
                        case "group":
                            if (current is null) break;
                            var group = fields[Object] switch
                            {
                                "G_Zug" => "Freight",
                                "P_Zug" => "Passenger",
                                var value when value.HasValue => value,
                                _ => null,
                            };
                            if (group.HasValue) current.Groups.Add(group);
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
        else
            return ImportResult<Timetable>.Success(result, messages);

        static IEnumerable<Message> AddTrain(Timetable timetable, Train train, int rowNumber)
        {
            if (train.Calls.Count == 0)
            {
                return [Message.Error(string.Format(CultureInfo.CurrentCulture, Resources.Strings.TrainHasNoCalls, rowNumber, train))];
            }
            else
            {
                timetable.Add(train.WithFixedSingleCallTrain().WithFirstCallDepartureOnlyAndLastCallArrivalOnly());
                return [];
            }

        }

        static Train CreateTrain(int rowNumber, string[] fields, TrainCategory? category)
        {
            var train = new Train(rowNumber, fields[Object].NumberOrZero, fields[Object])
            {
                Remark = fields[Remark],
                Category = category,
                CategoryId = category?.Id
            };
            return train;
        }

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

    private ImportResult<Schedule> GetSchedule(string name, Timetable timetable)
    {
        const string WorkSheetNameAndObjects = "Trains:locomotive,trainset,job,remarks";
        const string WorkSheetName = "Trains";
        const int TrainNumber = 0;
        const int From = 2;
        const int To = 3;
        const int Arrival = 4;
        const int Departure = 5;
        const int Group = 6;
        const int Object = 7;
        const int Type = 8;
        const int TrainName = 9;
        const int MinLength = 9;
        const int LocoClass = 9;
        const int TrainsetClass = 9;
        const int Remark = 10;

        var messages = new List<Message>();
        var locoSchedules = new Dictionary<string, VehicleSchedule>(100);
        var trainsetSchedules = new Dictionary<string, VehicleSchedule>(200);
        var driverDuties = new Dictionary<string, DriverDuty>();

        var trains = DataSet?.Tables[WorkSheetName];
        if (trains is null)
        {
            messages.Add(Message.System(string.Format(CultureInfo.CurrentCulture, Resources.Strings.WorksheetNotFound, WorkSheetName)));
            return ImportResult<Schedule>.Failure(messages);
        }

        messages.Add(Message.Information(string.Format(CultureInfo.CurrentCulture, Resources.Strings.ReadingWorksheet, WorkSheetNameAndObjects)));
        var schedule = Schedule.Create(name, timetable);
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
                                            vehicleType: VehicleType.Locomotive,
                                            number: locoId.NumberOrZero,
                                            vehicleClass: fields[LocoClass],
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
                                            TrainPart trainPart = new TrainPart(keys.FromCall.Value, keys.ToCall.Value);
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
                                                vehicleType: VehicleType.Trainset,
                                                number: trainsetId.NumberOrZero,
                                                vehicleClass: fields[TrainsetClass],
                                                externalId: trainsetId,
                                                remark: fields[Object].WithQuotationMarksRemoved);
                                            trainsetSchedules.Add(trainsetId, vehicleSchedule);
                                        }
                                        if (trainsetSchedules.TryGetValue(trainsetId, out var trainset))
                                        {
                                            var keys = GetTrainPartKeys(fields, currentTrain, rowNumber);
                                            trainsetMessages.AddRange(keys.Messages);
                                            if (trainsetMessages.HasNoStoppingErrors())
                                            {
                                                TrainPart trainPart = new TrainPart(keys.FromCall.Value, keys.ToCall.Value);
                                                trainset.Add(trainPart);
                                            }
                                        }
                                    }
                                    else // This might be a wagon group
                                    {
                                        if (fields[Object].IsEmpty && fields[Remark].IsEmpty) continue; // No information about wagon group, despite a row in the data.
                                        var wagonGroup = currentTrain.CreateWagonGroup(rowNumber, fields[Arrival].AsTime(), fields[Departure].AsTime(), fields[Group].ToIntOrZero, fields[Remark]);
                                        currentTrain.Add(wagonGroup);
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
                                            TrainPart trainPart = new TrainPart(keys.FromCall.Value, keys.ToCall.Value);
                                            duty.Add(trainPart);
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
        if (messages.HasStoppingErrors()) return ImportResult<Schedule>.Failure(messages);
        // Vehicles and VehicleSchedules are already added by CreateVehicleWithAllSessionsSchedule
        foreach (var duty in driverDuties.Values) schedule.AddDriverDuty(duty);
        return ImportResult<Schedule>.Success(schedule, messages);

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
