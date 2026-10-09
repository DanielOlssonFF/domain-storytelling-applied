namespace TE.DomainStorytellingApplied.Domain;

public sealed class Premises
{
    public Premises(string name, BookingPolicy bookingPolicy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(bookingPolicy);

        Id = Guid.NewGuid();
        Name = name;
        BookingPolicy = bookingPolicy;
    }

    public Guid Id { get; }

    public string Name { get; }

    public BookingPolicy BookingPolicy { get; }
}
