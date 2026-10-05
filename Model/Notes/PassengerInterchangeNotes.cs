namespace Tellurian.Trains.Schedules.Model.Notes;

/// <summary>
/// The passengers going on by another train are handed over here.
/// </summary>
public sealed record ExchangeTransferringPassengersNote : GeneratedNote;

/// <summary>
/// Derives the note a train's calls get at the stations where passengers change trains.
/// </summary>
/// <remarks>
/// <para>
/// An arrival note, because handing the passengers over is the first thing that happens once the train
/// has pulled in — and because the driver bringing them is the one who has to do it. It sits just after
/// the notes saying whether there is work here at all and before the lock keys, so it is read early.
/// </para>
/// <para>
/// Only a passenger train gets it, and only where it stops: a train carrying nobody has nobody to hand
/// over, and one running through hands over nothing.
/// </para>
/// </remarks>
public static class PassengerInterchangeNoteExtensions
{
    /// <summary>Where the note sits among a call's others. See the remarks above.</summary>
    private const int PassengerInterchangeDisplayOrder = 150;

    extension(StationCall call)
    {
        /// <summary>
        /// The passenger interchange note for this call, or nothing where the passengers are not changed
        /// over here.
        /// </summary>
        public IEnumerable<ICallNote> PassengerInterchangeNotes
        {
            get
            {
                call = call.ValueOrException(nameof(call));
                // IsPassenger is derived from the category, so it has to be asked rather than matched.
                if (call.Train is not { } train || !train.IsPassenger) return [];
                if (!call.IsStop || !call.OperationLocation.ExchangesTransferringPassengers) return [];
                return
                [
                    new ExchangeTransferringPassengersNote
                    {
                        IsForArrival = true,
                        DisplayOrder = PassengerInterchangeDisplayOrder,
                    }
                ];
            }
        }
    }
}
