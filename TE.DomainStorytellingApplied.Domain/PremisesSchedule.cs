namespace TE.DomainStorytellingApplied.Domain;

public sealed class PremisesSchedule
{
    private readonly List<Reservation> _reservations = [];

    public PremisesSchedule(Premises premises)
    {
        ArgumentNullException.ThrowIfNull(premises);

        Premises = premises;
    }

    public Premises Premises { get; }

    public IReadOnlyCollection<Reservation> Reservations => _reservations.AsReadOnly();

    public Reservation Reserve(Booker booker, Timeslot timeslot, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(booker);
        ArgumentNullException.ThrowIfNull(timeslot);

        if (!Premises.BookingPolicy.Allows(booker, timeslot, now))
        {
            throw new InvalidOperationException("The booking policy does not allow this reservation.");
        }

        if (_reservations.Any(r => r.Status != ReservationStatus.Rejected && r.Timeslot.Overlaps(timeslot)))
        {
            throw new InvalidOperationException("The timeslot overlaps an existing reservation.");
        }

        var reservation = new Reservation(booker, Premises, timeslot);
        reservation.Submit();
        _reservations.Add(reservation);

        return reservation;
    }
}
