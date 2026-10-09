namespace TE.DomainStorytellingApplied.Domain;

public sealed class Notification
{
    private Notification(Booker recipient, Guid reservationId, NotificationType type, string message)
    {
        Recipient = recipient;
        ReservationId = reservationId;
        Type = type;
        Message = message;
    }

    public Booker Recipient { get; }

    public Guid ReservationId { get; }

    public NotificationType Type { get; }

    public string Message { get; }

    public static Notification For(Reservation reservation)
    {
        ArgumentNullException.ThrowIfNull(reservation);

        return reservation.Status switch
        {
            ReservationStatus.Approved => new Notification(
                reservation.Booker,
                reservation.Id,
                NotificationType.Confirmation,
                $"Your reservation of {reservation.Premises.Name} has been confirmed."),
            ReservationStatus.Rejected => new Notification(
                reservation.Booker,
                reservation.Id,
                NotificationType.Rejection,
                $"Your reservation of {reservation.Premises.Name} has been rejected. Reason: {reservation.RejectionReason}"),
            _ => throw new InvalidOperationException(
                $"Cannot notify about a reservation with status {reservation.Status}.")
        };
    }
}
