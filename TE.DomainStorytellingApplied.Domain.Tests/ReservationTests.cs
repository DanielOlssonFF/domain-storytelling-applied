namespace TE.DomainStorytellingApplied.Domain.Tests;

public class ReservationTests
{
    private static readonly Booker Booker =
        new("Anna Andersson", "anna@example.com", BookerType.PrivateIndividual);

    private static readonly Approver Approver = new("Bertil Berg");

    private static readonly Timeslot Timeslot =
        new(new DateTime(2030, 1, 2, 10, 0, 0), new DateTime(2030, 1, 2, 12, 0, 0));

    private static Premises CreatePremises(bool requiresReview) =>
        new("Stora salen", new BookingPolicy(
            [BookerType.PrivateIndividual],
            requiresReview,
            minDuration: TimeSpan.FromHours(1),
            maxDuration: TimeSpan.FromHours(4),
            opensAt: new TimeOnly(8, 0),
            closesAt: new TimeOnly(22, 0)));

    private static Reservation CreatePendingReservation()
    {
        var reservation = new Reservation(Booker, CreatePremises(requiresReview: true), Timeslot);
        reservation.Submit();
        return reservation;
    }

    [Fact]
    public void GivenValidValues_WhenCreatingReservation_ShouldHaveStatusCreated()
    {
        var premises = CreatePremises(requiresReview: false);

        var reservation = new Reservation(Booker, premises, Timeslot);

        reservation.Id.ShouldNotBe(Guid.Empty);
        reservation.Booker.ShouldBe(Booker);
        reservation.Premises.ShouldBe(premises);
        reservation.Timeslot.ShouldBe(Timeslot);
        reservation.Status.ShouldBe(ReservationStatus.Created);
        reservation.ReviewedBy.ShouldBeNull();
        reservation.RejectionReason.ShouldBeNull();
    }

    [Fact]
    public void GivenNoBooker_WhenCreatingReservation_ShouldThrow()
    {
        Should.Throw<ArgumentNullException>(() => new Reservation(null!, CreatePremises(false), Timeslot));
    }

    [Fact]
    public void GivenNoPremises_WhenCreatingReservation_ShouldThrow()
    {
        Should.Throw<ArgumentNullException>(() => new Reservation(Booker, null!, Timeslot));
    }

    [Fact]
    public void GivenNoTimeslot_WhenCreatingReservation_ShouldThrow()
    {
        Should.Throw<ArgumentNullException>(() => new Reservation(Booker, CreatePremises(false), null!));
    }

    [Fact]
    public void GivenPolicyWithoutReview_WhenSubmitting_ShouldBeApproved()
    {
        var reservation = new Reservation(Booker, CreatePremises(requiresReview: false), Timeslot);

        reservation.Submit();

        reservation.Status.ShouldBe(ReservationStatus.Approved);
        reservation.ReviewedBy.ShouldBeNull();
    }

    [Fact]
    public void GivenPolicyWithReview_WhenSubmitting_ShouldBePendingReview()
    {
        var reservation = new Reservation(Booker, CreatePremises(requiresReview: true), Timeslot);

        reservation.Submit();

        reservation.Status.ShouldBe(ReservationStatus.PendingReview);
    }

    [Fact]
    public void GivenSubmittedReservation_WhenSubmittingAgain_ShouldThrow()
    {
        var reservation = CreatePendingReservation();

        Should.Throw<InvalidOperationException>(() => reservation.Submit());
    }

    [Fact]
    public void GivenPendingReview_WhenApproverApproves_ShouldBeApprovedAndReviewedByApprover()
    {
        var reservation = CreatePendingReservation();

        reservation.Approve(Approver);

        reservation.Status.ShouldBe(ReservationStatus.Approved);
        reservation.ReviewedBy.ShouldBe(Approver);
    }

    [Fact]
    public void GivenPendingReview_WhenApproverRejects_ShouldBeRejectedWithReason()
    {
        var reservation = CreatePendingReservation();

        reservation.Reject(Approver, "Lokalen renoveras");

        reservation.Status.ShouldBe(ReservationStatus.Rejected);
        reservation.ReviewedBy.ShouldBe(Approver);
        reservation.RejectionReason.ShouldBe("Lokalen renoveras");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void GivenPendingReview_WhenRejectingWithoutReason_ShouldThrow(string reason)
    {
        var reservation = CreatePendingReservation();

        Should.Throw<ArgumentException>(() => reservation.Reject(Approver, reason));
    }

    [Fact]
    public void GivenPendingReview_WhenApprovingWithoutApprover_ShouldThrow()
    {
        var reservation = CreatePendingReservation();

        Should.Throw<ArgumentNullException>(() => reservation.Approve(null!));
    }

    [Fact]
    public void GivenCreatedReservation_WhenApproving_ShouldThrow()
    {
        var reservation = new Reservation(Booker, CreatePremises(requiresReview: true), Timeslot);

        Should.Throw<InvalidOperationException>(() => reservation.Approve(Approver));
    }

    [Fact]
    public void GivenApprovedReservation_WhenReviewingAgain_ShouldThrow()
    {
        var reservation = CreatePendingReservation();
        reservation.Approve(Approver);

        Should.Throw<InvalidOperationException>(() => reservation.Approve(Approver));
        Should.Throw<InvalidOperationException>(() => reservation.Reject(Approver, "För sent"));
    }

    [Fact]
    public void GivenRejectedReservation_WhenReviewingAgain_ShouldThrow()
    {
        var reservation = CreatePendingReservation();
        reservation.Reject(Approver, "Lokalen renoveras");

        Should.Throw<InvalidOperationException>(() => reservation.Approve(Approver));
        Should.Throw<InvalidOperationException>(() => reservation.Reject(Approver, "Igen"));
    }
}
