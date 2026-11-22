namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>The problems found while working on the lines of a layout, each once, in the order found.</summary>
internal sealed class LineProblemLog
{
    private readonly List<LineProblem> _problems = [];
    private readonly HashSet<LineProblem> _reported = [];

    public IReadOnlyList<LineProblem> Problems => _problems;

    public void Report(LineProblem problem)
    {
        if (_reported.Add(problem)) _problems.Add(problem);
    }
}
