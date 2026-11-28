namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>Whether there are signals at an <see cref="OperationLocation"/>, whatever its kind.</summary>
public static class OperationLocationSignals
{
    extension(OperationLocation location)
    {
        /// <summary>
        /// Whether there are signals at the location: as entered for a <see cref="Station"/> or an
        /// <see cref="IndustrialArea"/>; always at a <see cref="SignalControlledLocation"/>, which has signals by design;
        /// never at an <see cref="OtherLocation"/>.
        /// </summary>
        public bool HasSignals => location switch
        {
            Station station => station.HasSignals,
            IndustrialArea area => area.HasSignals,
            SignalControlledLocation => true,
            OtherLocation => false,
        };
    }
}
