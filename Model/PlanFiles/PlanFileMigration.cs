using System.Text.Json.Nodes;

namespace Tellurian.Trains.Schedules.Model.PlanFiles;

/// <summary>
/// One step of the chain that brings the content of a plan file from schema version <paramref name="FromVersion"/>
/// to the next. A step works on the JSON tree, before the model reads it, so the model never has to know an older
/// format. A step is written once and never changed.
/// </summary>
/// <param name="FromVersion">The schema version the step reads.</param>
/// <param name="Migrate">Changes the content of a plan of <paramref name="FromVersion"/> into the next version.</param>
public sealed record PlanFileMigration(int FromVersion, Func<JsonNode, JsonNode> Migrate);
