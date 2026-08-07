namespace Tellurian.Trains.Schedules.Planning.Components.Reporting;

/// <summary>
/// One font family a report may be set in.
/// </summary>
/// <param name="Name">The family name exactly as the operating system knows it, which is also what
/// is stored in the layout's settings.</param>
/// <param name="Group">Where it is filed in the list, and what it falls back to when not installed.</param>
public sealed record ReportFont(string Name, ReportFontGroup Group);
