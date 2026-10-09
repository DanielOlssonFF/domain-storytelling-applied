namespace TE.DomainStorytellingApplied.Domain.Tests;

public class NotificationTests
{
    private static readonly Booker Booker =
        new("Anna Andersson", "anna@example.com", BookerType.PrivateIndividual);

    private static readonly Approver Approver = new("Bertil Berg");

    private static readonly Timeslot Timeslot =
        new(new DateTime(2030, 1, 2, 10, 0, 0), new DateTime(2030, 1, 2, 12, 0, 0));

    private static Reservation CreateReservation(bool requiresReview) =>
        new(Booker, new Premises("Stora salen", 50, new BookingPolicy(
            [BookerType.PrivateIndividual],
            requiresReview,
            minDuration: TimeSpan.FromHours(1),
            maxDuration: TimeSpan.FromHours(4),
            opensAt: new TimeOnly(8, 0),
            closesAt: new TimeOnly(22, 0))), Timeslot);

    [Fact]
    public void GivenAutomaticallyApprovedReservation_WhenCreatingNotification_ShouldBeConfirmationToBooker()
    {
        var reservation = CreateReservation(requiresReview: false);
        reservation.Submit();

        var notification = Notification.For(reservation);

        notification.Type.ShouldBe(NotificationType.Confirmation);
        notification.Recipient.ShouldBe(Booker);
        notification.ReservationId.ShouldBe(reservation.Id);
        notification.Message.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public void GivenReservationApprovedByApprover_WhenCreatingNotification_ShouldBeConfirmation()
    {
        var reservation = CreateReservation(requiresReview: true);
        reservation.Submit();
        reservation.Approve(Approver);

        var notification = Notification.For(reservation);

        notification.Type.ShouldBe(NotificationType.Confirmation);
        notification.Recipient.ShouldBe(Booker);
    }

    [Fact]
    public void GivenRejectedReservation_WhenCreatingNotification_ShouldBeRejectionContainingReason()
    {
        var reservation = CreateReservation(requiresReview: true);
        reservation.Submit();
        reservation.Reject(Approver, "Lokalen renoveras");

        var notification = Notification.For(reservation);

        notification.Type.ShouldBe(NotificationType.Rejection);
        notification.Recipient.ShouldBe(Booker);
        notification.ReservationId.ShouldBe(reservation.Id);
        notification.Message.ShouldContain("Lokalen renoveras");
    }

    [Fact]
    public void GivenPendingReviewReservation_WhenCreatingNotification_ShouldThrow()
    {
        var reservation = CreateReservation(requiresReview: true);
        reservation.Submit();

        Should.Throw<InvalidOperationException>(() => Notification.For(reservation));
    }

    [Fact]
    public void GivenCreatedReservation_WhenCreatingNotification_ShouldThrow()
    {
        var reservation = CreateReservation(requiresReview: false);

        Should.Throw<InvalidOperationException>(() => Notification.For(reservation));
    }

    [Fact]
    public void GivenNoReservation_WhenCreatingNotification_ShouldThrow()
    {
        Should.Throw<ArgumentNullException>(() => Notification.For(null!));
    }
}
