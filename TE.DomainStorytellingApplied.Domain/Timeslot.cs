namespace TE.DomainStorytellingApplied.Domain;

public sealed record Timeslot
{
    public Timeslot(DateTime start, DateTime end)
    {
        if (start >= end)
        {
            throw new ArgumentException("Start must be before end.", nameof(start));
        }

        Start = start;
        End = end;
    }

    public DateTime Start { get; }

    public DateTime End { get; }

    public TimeSpan Duration => End - Start;

    public bool Overlaps(Timeslot other)
    {
        ArgumentNullException.ThrowIfNull(other);

        return Start < other.End && other.Start < End;
    }
}
