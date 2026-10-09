namespace TE.DomainStorytellingApplied.Domain;

public sealed class Reservation
{
    internal Reservation(Booker booker, Premises premises, Timeslot timeslot)
    {
        ArgumentNullException.ThrowIfNull(booker);
        ArgumentNullException.ThrowIfNull(premises);
        ArgumentNullException.ThrowIfNull(timeslot);

        Id = Guid.NewGuid();
        Booker = booker;
        Premises = premises;
        Timeslot = timeslot;
        Status = ReservationStatus.Created;
    }

    public Guid Id { get; }

    public Booker Booker { get; }

    public Premises Premises { get; }

    public Timeslot Timeslot { get; }

    public ReservationStatus Status { get; private set; }

    public Approver? ReviewedBy { get; private set; }

    public string? RejectionReason { get; private set; }

    internal void Submit()
    {
        EnsureStatus(ReservationStatus.Created);

        Status = Premises.BookingPolicy.RequiresReview
            ? ReservationStatus.PendingReview
            : ReservationStatus.Approved;
    }

    public void Approve(Approver approver)
    {
        ArgumentNullException.ThrowIfNull(approver);
        EnsureStatus(ReservationStatus.PendingReview);

        Status = ReservationStatus.Approved;
        ReviewedBy = approver;
    }

    public void Reject(Approver approver, string reason)
    {
        ArgumentNullException.ThrowIfNull(approver);
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        EnsureStatus(ReservationStatus.PendingReview);

        Status = ReservationStatus.Rejected;
        ReviewedBy = approver;
        RejectionReason = reason;
    }

    private void EnsureStatus(ReservationStatus expected)
    {
        if (Status != expected)
        {
            throw new InvalidOperationException(
                $"Reservation must have status {expected} but has status {Status}.");
        }
    }
}
