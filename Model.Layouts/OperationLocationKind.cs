namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// What kind of place an <see cref="OperationLocation"/> is: the value of its type in a file or a database. The kind decides what the location can have
/// (<see cref="OperationLocationRules"/>). The numbers are stored by the Module Registry and never change.
/// </summary>
public enum OperationLocationKind
{
    /// <summary>A station: trains stop to meet, be overtaken, or exchange passengers or cargo. It can be manned.</summary>
    Station = 0,

    /// <summary>A block post, junction or crossing place worked by signals. Trains never stop there.</summary>
    SignalControlled = 1,

    /// <summary>A freight-only area: cargo is always exchanged, passengers never.</summary>
    IndustrialArea = 2,

    /// <summary>Another place where trains can stop, such as an unstaffed halt. No cargo.</summary>
    Other = 3,
}

/// <summary>The kind of an <see cref="OperationLocation"/>, worked out from its type.</summary>
public static class OperationLocationKindExtensions
{
    extension(OperationLocation location)
    {
        /// <summary>What kind of place the location is: its type, as the value stored in a file or a database.</summary>
        public OperationLocationKind Kind => location switch
        {
            Station => OperationLocationKind.Station,
            SignalControlledLocation => OperationLocationKind.SignalControlled,
            IndustrialArea => OperationLocationKind.IndustrialArea,
            OtherLocation => OperationLocationKind.Other,
        };
    }
}
