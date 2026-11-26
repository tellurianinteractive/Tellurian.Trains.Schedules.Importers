namespace Tellurian.Trains.Schedules.Model.Notes;

/// <summary>
/// Note naming the train's category on the station dispatch list, where the train stops and its category
/// asks for it (see <see cref="TrainCategory.ShowNameOnDispatchList"/>).
/// </summary>
/// <remarks>
/// It leads the notes: the kind of train is what the station reads the rest of the row in the light of. It
/// is for the station only; the loco driver knows what train they drive.
/// </remarks>
/// <param name="Name">The category's name.</param>
public sealed record TrainCategoryNote(string Name) : GeneratedNote;
