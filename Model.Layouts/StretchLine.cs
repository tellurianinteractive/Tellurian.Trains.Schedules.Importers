namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// A line of a hall plan as the planner keeps it: the order of the locations along it, as legs between neighbouring
/// locations, in the direction of the line (<c>ModelSurfaceAnalysis.md</c> 3.3). It is worked out when a hall plan is
/// read (<c>layout.UpdateTrackStretchesFromLines()</c>), so that the planner can suggest timetable stretches without the
/// drawings. A branch starts at the location it leaves from. Where the walk along the line found no stretch, the legs
/// have a gap: one leg does not start where the one before it ended.
/// </summary>
public sealed class StretchLine
{
    /// <summary>The id of the line in the hall plan.</summary>
    public int Id { get; set; }

    /// <summary>The name of the line, if it has one.</summary>
    public string? Name { get; set; }

    /// <summary>The legs between neighbouring locations, in the order and direction of the line.</summary>
    public IList<LineLeg> Legs { get; set; } = [];

    /// <summary>A copy of the line, with copies of its legs.</summary>
    public StretchLine Copy() => new() { Id = Id, Name = Name, Legs = [.. Legs.Select(l => l with { })] };

    /// <inheritdoc/>
    public override string ToString() => Name ?? Id.ToString(System.Globalization.CultureInfo.InvariantCulture);
}

/// <summary>A leg of a <see cref="StretchLine"/>: from one location to its neighbour, in the direction of the line.</summary>
/// <param name="StartId">The <see cref="OperationLocation.Id"/> of the location the leg starts at.</param>
/// <param name="EndId">The <see cref="OperationLocation.Id"/> of the location the leg ends at.</param>
public sealed record LineLeg(int StartId, int EndId)
{
    /// <summary>Whether the leg runs between <paramref name="a"/> and <paramref name="b"/>, in either direction.</summary>
    public bool Joins(int a, int b) => (StartId == a && EndId == b) || (StartId == b && EndId == a);
}

/// <summary>What keeps the <see cref="Layout.StretchLines"/> in step when locations come and go.</summary>
public static class StretchLineChanges
{
    extension(Layout layout)
    {
        /// <summary>
        /// Puts <paramref name="inserted"/> into every leg between <paramref name="a"/> and <paramref name="b"/>, so that
        /// the leg becomes two, in the same direction.
        /// </summary>
        public void InsertIntoStretchLines(OperationLocation a, OperationLocation b, OperationLocation inserted)
        {
            ArgumentNullException.ThrowIfNull(a);
            ArgumentNullException.ThrowIfNull(b);
            ArgumentNullException.ThrowIfNull(inserted);
            foreach (var line in layout.StretchLines)
            {
                for (var i = line.Legs.Count - 1; i >= 0; i--)
                {
                    var leg = line.Legs[i];
                    if (!leg.Joins(a.Id, b.Id)) continue;
                    line.Legs[i] = new(leg.StartId, inserted.Id);
                    line.Legs.Insert(i + 1, new(inserted.Id, leg.EndId));
                }
            }
        }

        /// <summary>
        /// Takes <paramref name="removed"/> out of every line: two legs that meet there become one, and a leg that
        /// starts or ends there with no leg to join is dropped.
        /// </summary>
        public void RemoveFromStretchLines(OperationLocation removed)
        {
            ArgumentNullException.ThrowIfNull(removed);
            foreach (var line in layout.StretchLines)
            {
                var legs = new List<LineLeg>();
                foreach (var leg in line.Legs)
                {
                    if (leg.StartId == removed.Id && legs.Count > 0 && legs[^1].EndId == removed.Id)
                        legs[^1] = new(legs[^1].StartId, leg.EndId);
                    else
                        legs.Add(leg);
                }
                line.Legs = [.. legs.Where(l => l.StartId != removed.Id && l.EndId != removed.Id)];
            }
        }
    }
}
