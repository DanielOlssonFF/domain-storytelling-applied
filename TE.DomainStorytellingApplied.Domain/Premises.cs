namespace TE.DomainStorytellingApplied.Domain;

public sealed class Premises
{
    public Premises(string name, int capacity, BookingPolicy bookingPolicy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(bookingPolicy);

        if (capacity <= 0)
        {
            throw new ArgumentException("Capacity must be greater than zero.", nameof(capacity));
        }

        Id = Guid.NewGuid();
        Name = name;
        Capacity = capacity;
        BookingPolicy = bookingPolicy;
    }

    public Guid Id { get; }

    public string Name { get; }

    public int Capacity { get; }

    public BookingPolicy BookingPolicy { get; }
}
