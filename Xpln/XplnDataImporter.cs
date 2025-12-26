using Microsoft.Extensions.Logging;
using System.Data;
using System.Globalization;
using System.Text;
using Tellurian.Trains.Schedules.Importers.Interfaces;
using Tellurian.Trains.Schedules.Importers.Model;
using Tellurian.Trains.Schedules.Importers.Services;
using Tellurian.Trains.Schedules.Importers.Xpln.DataSetProviders;
using Tellurian.Trains.Schedules.Importers.Xpln.Extensions;
using Tellurian.Trains.Schedules.Model;
using static Tellurian.Trains.Schedules.Model.TrainExtensions;

namespace Tellurian.Trains.Schedules.Importers.Xpln;

public sealed class XplnDataImporter : IImportService, IDisposable
{
    internal record TrainPartKeys(Maybe<StationCall> FromCall, Maybe<StationCall> ToCall, IEnumerable<Message> Messages);

    private readonly Stream _inputStream;
    private readonly IDataSetProvider _dataSetProvider;
    private readonly ILogger _logger;
    private readonly IOperatingCompaniesService _operatingCompaniesService;
    private readonly ITrainCategoriesService _trainCategoriesService;
    private readonly DataSetConfiguration _dataSetConfiguration = CreateDataSetConfiguration();
    private List<OperatingCompany> _operatingCompanies = [];
    private List<TrainCategory> _trainCategories = [];
    private DataSet? DataSet;

    public XplnDataImporter(Stream inputStream, IDataSetProvider dataSetProvider, IOperatingCompaniesService operatingCompaniesService, ITrainCategoriesService trainCategoriesService, ILogger<XplnDataImporter> logger)
    {
        _inputStream = inputStream;
        _dataSetProvider = dataSetProvider;
        _operatingCompaniesService = operatingCompaniesService;
        _trainCategoriesService = trainCategoriesService; ;
        _logger = logger;
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    public XplnDataImporter(FileInfo inputFile, IDataSetProvider dataSetProvider, IOperatingCompaniesService operatingCompaniesService, ITrainCategoriesService trainCategoriesService, ILogger<XplnDataImporter> logger) :
        this(File.OpenRead(inputFile.FullName), dataSetProvider, operatingCompaniesService, trainCategoriesService, logger)
    { }

    private OperatingCompany FindOrCreateCompany(string? companySignature)
    {
        if (companySignature.HasValue())
        {
            _ = _operatingCompanies.TryGetFirstValue(oc => oc.Signature.EqualsCaseInsensitive(companySignature), out var company);
            if (company is not null) return company;
            var newCompany = OperatingCompany.FromSignature(companySignature);
            _operatingCompanies.Add(newCompany);
            return newCompany;
        }
        return OperatingCompany.None;
    }

    private TrainCategory FindOrCreateCategory(string? trainPrefix, string? backgroundColor)
    {
        _ = _trainCategories.TryGetFirstValue(tc => tc.Prefix.EqualsCaseInsensitive(trainPrefix), out var category);
        if (category is not null) return category;
        if (trainPrefix.HasValue())
        {
            var newCategory = new TrainCategory() { ResourceName = trainPrefix, Prefix = trainPrefix, Color = backgroundColor ?? "#FFFFFF" };
            _trainCategories.Add(newCategory);
            return newCategory;
        }
        return TrainCategory.Unknown;
    }


    public async Task<ImportResult<Schedule>> ImportSchedule(string name)
    {
        _operatingCompanies = [.. await _operatingCompaniesService.GetAllOperatingCompaies()];
        _trainCategories = [.. await _trainCategoriesService.GetAllTrainCategoriesAsync()];
        DataSet = _dataSetProvider.ImportSchedule(_inputStream, _dataSetConfiguration) ?? throw new IOException("Stream cannot be read.");
        return GetResult(name);
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

    private ImportResult<Schedule> GetResult(string name)
    {
        var layout = GetLayout(name);
        if (layout.IsFailure)
        {
            var result = new ImportResult<Schedule>() { Name = name, Messages = layout.Messages };
            LogMessages(result.Messages);
            return result;
        }
        var timetable = GetTimetable(name, layout.Item);
        if (timetable.IsFailure)
        {
            var result = new ImportResult<Schedule>() { Name = name, Messages = [.. layout.Messages, .. timetable.Messages] };
            LogMessages(result.Messages);
            return result;
        }
        var schedule = GetSchedule(name, timetable.Item);
        var ImportResult = schedule with { Name = name, Messages = [.. layout.Messages, .. timetable.Messages, .. schedule.Messages] };
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
        return routes;

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
        foreach (DataRow station in stations.Rows)
        {
            if (rowNumber > 1)
            {
                if (IsRepeatedHeader(station)) continue;
                var itemMessages = new List<Message>();
                var fields = station.GetRowFields();
                if (fields.IsEmptyFields()) { if (layout.Stations.Count > 0) break; else continue; }
                itemMessages.AddRange(ValidateRow(fields, rowNumber));
                if (itemMessages.HasNoStoppingErrors())
                {
                    if (fields[5].Is("Station"))
                    {
                        if (current is not null)
                        {
                            layout.Add(current);
                            current = null;
                        }
                        var validationMessages = ValidateStation(fields, rowNumber);
                        if (validationMessages.HasNoStoppingErrors())
                        {
                            current = CreateStation(rowNumber, fields);
                        }
                        itemMessages.AddRange(validationMessages);
                    }
                    else if (fields[5].Is("Track"))
                    {
                        if (current is null) continue;
                        var validationMessages = ValidateTrack(fields, rowNumber);
                        if (validationMessages.HasNoStoppingErrors())
                        {
                            current.Add(CreateTrack(fields));
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

        static OperationLocation CreateStation(int rowNumber, string[] fields) =>
            new(rowNumber, fields[Name], fields[Signature])
            {
                Type = fields[Type],
                IsShadow = fields[SubType].Is("Depot")
            };

        static StationTrack CreateTrack(string[] fields) =>
            new(fields[TrackName])
            {
                IsMain = fields[SubType].Is("Main"),
                IsScheduled = fields[SubType].IsAny(["Main", "Depot"]),
                Usage = fields[Remark],
                DisplayOrder = fields[1].NumberOrZero,
            };

        static Message[] ValidateRow(string[] fields, int rowNumber)
        {
            var messages = new List<Message>();
            if (fields.Length < MinLength)
                messages.Add(Message.Error(Resources.Strings.NotAllFieldsArePresent, rowNumber, MinLength, fields.Length));
            if (!fields[Type].ValueOrEmpty().IsAny(["Station", "Track"]))
                messages.Add(Message.Error(Resources.Strings.UnsupportedType, rowNumber, fields[Type]));
            return [.. messages];
        }

        static Message[] ValidateStation(string[] fields, int rowNumber)
        {
            var messages = new List<Message>();
            if (fields[Signature].IsEmpty())
                messages.Add(Message.Error(Resources.Strings.ColumnMustHaveAValue, rowNumber, "Name"));
            if (!fields[SubType].ValueOrEmpty().IsAny(["Station", "Block"]))
                messages.Add(Message.Error(Resources.Strings.UnsupportedSubType, rowNumber, fields[SubType]));
            return [.. messages];
        }

        static Message[] ValidateTrack(string[] fields, int rowNumber)
        {
            var messages = new List<Message>();
            if (fields[TrackName].IsEmpty())
                messages.Add(Message.Error(Resources.Strings.ColumnMustHaveAValue, rowNumber, "TrackName"));
            if (!fields[Lenght].IsEmpty() && !fields[Lenght].IsNumber())
                messages.Add(Message.Error(Resources.Strings.ColumnMustBeANumber, rowNumber, "Length", fields[Lenght]));
            if (!fields[SubType].ValueOrEmpty().IsAny(["Main", "Side", "Siding", "Depot", "Goods"]))
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
                if (fields.IsEmptyFields()) { if (layout.Stations.Count > 0) break; else continue; }
                if (fields[StartStation].IsZeroOrEmpty() && fields[EndStation].IsZeroOrEmpty()) continue;

                var start = layout.Station(fields[StartStation]);
                var end = layout.Station(fields[EndStation]);
                if (start.IsNone)
                    itemMessages.Add(Message.Error(Resources.Strings.StationNotFoundInLayout, rowNumber, fields[StartStation]));
                if (end.IsNone)
                    itemMessages.Add(Message.Error(Resources.Strings.StationNotFoundInLayout, rowNumber, fields[EndStation]));
                if (!fields[Tracks].IsNumber())
                    itemMessages.Add(Message.Error(Resources.Strings.ColumnMustBeANumber, rowNumber, nameof(Tracks), fields[Tracks]));
                if (!fields[Speed].IsNumber())
                    itemMessages.Add(Message.Error(Resources.Strings.ColumnMustBeANumber, rowNumber, nameof(Speed), fields[Speed]));
                if (!fields[Time].IsNumber())
                    itemMessages.Add(Message.Error(Resources.Strings.ColumnMustBeANumber, rowNumber, nameof(Time), fields[Time]));
                if (!fields[EndPosition].IsNumber())
                    itemMessages.Add(Message.Error(Resources.Strings.ColumnMustBeANumber, rowNumber, nameof(EndPosition), fields[EndPosition]));
                if (itemMessages.HasNoStoppingErrors())
                {
                    TimetableStretch? timetableStretch = null;
                    var routeNumber = fields[Route].HasText() ? fields[Route] : DefaultRoute;
                    if (fields[Route].IsEmpty())
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
                        var distance = Math.Abs(fields[EndPosition].ToDouble() - fields[StartPosition].ToDouble());
                        var stretch = new TrackStretch(rowNumber, start.Value, end.Value, distance, fields[Tracks].ToInteger(), fields[Speed].ToInteger(), fields[Time].ToInteger());
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
                if (fields.IsEmptyFields()) { if (result.Trains.Count > 0) break; else continue; }
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

                                if (fields[Object].HasValue())
                                {
                                    var note = new Note()
                                    {
                                        IsDriverNote = true,
                                        IsStationNote = true,
                                        LanguageCode = CultureInfo.CurrentCulture.TwoLetterISOLanguageName,
                                        Text = fields[Remark].HasValue() ?
                                            string.Format(CultureInfo.CurrentCulture, Resources.Strings.UseLocoClasses, fields[Object], fields[Remark]) :
                                            string.Format(CultureInfo.CurrentCulture, Resources.Strings.UseLoco, fields[Object])
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
                                if (fields[Remark].HasValue())
                                {
                                    var note = new Note()
                                    {
                                        IsDriverNote = true,
                                        LanguageCode = CultureInfo.CurrentCulture.TwoLetterISOLanguageName,
                                        Text = $"{fields[Object]} {fields[Remark]}",

                                    };
                                    current?.Calls.First().Notes.Add(note);
                                }
                                ;

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
                                var value when value.HasValue() => value,
                                _ => null,
                            };
                            if (group.HasValue()) current.Groups.Add(group);
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
                timetable.Add(train.WithFixedSingleCallTrain().WithFixedFirstAndLastCall());
                return [];
            }

        }

        static Train CreateTrain(int rowNumber, string[] fields, TrainCategory category)
        {
            return new(rowNumber, category, fields[Object].NumberOrZero, fields[Object])
            { Remark = fields[Remark] };
        }

        static StationCall CreateCall(int rowNumber, string[] fields, StationTrack track)
        {
            return new(track, fields[Arrival].AsTime(), fields[Departure].AsTime(), fields[Remark])
            {
                Id = rowNumber,
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
            else if (!fields[Type].IsAny(["Traindef", "Timetable", "Locomotive", "Trainset", "Job", "Wheel", "Group"]))
                messages.Add(Message.Error(string.Format(CultureInfo.CurrentCulture, Resources.Strings.UnsupportedType, rowNumber, fields[Type])));
            return [.. messages];
        }

        static Message[] ValidateTrain(string[] fields, int rowNumber)
        {
            var messages = new List<Message>();
            if (fields[Object].IsEmpty())
                messages.Add(Message.Error(string.Format(CultureInfo.CurrentCulture, Resources.Strings.ColumnMustHaveAValue, rowNumber, "Object")));
            return [.. messages];
        }

        static Message[] ValidateCall(string[] fields, int rowNumber)
        {
            var messages = new List<Message>();
            if (fields[Track].IsEmpty())
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
        const int Object = 7;
        const int Type = 8;
        const int TrainName = 9;
        const int MinLength = 9;
        const int LocoClass = 9;
        const int TrainsetClass = 9;

        var messages = new List<Message>();
        var locoSchedules = new Dictionary<string, LocoSchedule>(100);
        var trainsetSchedules = new Dictionary<string, TrainsetSchedule>(200);
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
                if (fields.IsEmptyFields()) { if (locoSchedules.Count > 0 || trainsetSchedules.Count > 0 || driverDuties.Count > 0) break; else continue; }
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
                                        locoSchedules.Add(locoId, new LocoSchedule(locoId.NumberOrZero) { Id = rowNumber, Company = company, Class = fields[LocoClass] });
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
                                    var trainsetId = fields[Object].OrElse(fields[TrainName]);
                                    if (!trainsetSchedules.ContainsKey(trainsetId))
                                        trainsetSchedules.Add(trainsetId, new TrainsetSchedule(trainsetId.NumberOrZero) { Id = rowNumber, Class = fields[TrainsetClass], Remark = fields[Object] });
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
        foreach (var loco in locoSchedules.Values) schedule.AddLocoSchedule(loco);
        foreach (var trainset in trainsetSchedules.Values) schedule.AddTrainsetSchedule(trainset);
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

        static string ObjectDescription(string[] fields) => fields[Object].HasText() ? $"{fields[Type]}:{fields[Object]}".Trim() : fields[Type];


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
            else if (!fields[Type].IsAny(["Traindef", "Timetable", "Locomotive", "Trainset", "Job", "Wheel", "Group"]))
                messages.Add(Message.Error(string.Format(CultureInfo.CurrentCulture, Resources.Strings.UnsupportedType, rowNumber, fields[Type])));
            return [.. messages];
        }

        static Message[] ValidateLoco(string[] fields, int rowNumber)
        {
            var messages = new List<Message>();
            if (fields[Object].IsEmpty())
                messages.Add(Message.Error(string.Format(CultureInfo.CurrentCulture, Resources.Strings.ColumnMustHaveAValue, rowNumber, "Object")));
            return [.. messages];
        }
        static Message[] ValidateJob(string[] fields, int rowNumber)
        {
            var messages = new List<Message>();
            if (fields[Object].OrElse(fields[TrainNumber]).IsEmpty())
                messages.Add(Message.Error(string.Format(CultureInfo.CurrentCulture, Resources.Strings.ColumnMustHaveAValue, rowNumber, "Object|TrainNumber")));
            return [.. messages];
        }
        static Message[] ValidateTrainset(string[] fields, int rowNumber)
        {
            var messages = new List<Message>();
            if (fields[Object].IsEmpty() && fields[TrainName].IsEmpty())
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

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    #endregion
}
