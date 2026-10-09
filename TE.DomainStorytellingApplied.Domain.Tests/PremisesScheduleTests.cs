namespace TE.DomainStorytellingApplied.Domain.Tests;

public class PremisesScheduleTests
{
    private static readonly DateTime Now = new(2030, 1, 1, 8, 0, 0);

    private static readonly Booker Booker =
        new("Anna Andersson", "anna@example.com", BookerType.PrivateIndividual);

    private static readonly Booker AssociationBooker =
        new("Cecilia Carlsson", "cecilia@example.com", BookerType.AssociationRepresentative);

    private static readonly Approver Approver = new("Bertil Berg");

    private static PremisesSchedule CreateSchedule(bool requiresReview = false) =>
        new(new Premises("Stora salen", 50, new BookingPolicy(
            [BookerType.PrivateIndividual],
            requiresReview,
            minDuration: TimeSpan.FromHours(1),
            maxDuration: TimeSpan.FromHours(4),
            opensAt: new TimeOnly(8, 0),
            closesAt: new TimeOnly(22, 0))));

    private static Timeslot Slot(int startHour, int endHour) =>
        new(new DateTime(2030, 1, 2, startHour, 0, 0), new DateTime(2030, 1, 2, endHour, 0, 0));

    [Fact]
    public void GivenNoPremises_WhenCreatingSchedule_ShouldThrow()
    {
        Should.Throw<ArgumentNullException>(() => new PremisesSchedule(null!));
    }

    [Fact]
    public void GivenFreeTimeslot_WhenReserving_ShouldCreateReservation()
    {
        var schedule = CreateSchedule();

        var reservation = schedule.Reserve(Booker, Slot(10, 12), Now);

        reservation.Booker.ShouldBe(Booker);
        reservation.Premises.ShouldBe(schedule.Premises);
        reservation.Timeslot.ShouldBe(Slot(10, 12));
        schedule.Reservations.ShouldContain(reservation);
    }

    [Fact]
    public void GivenPolicyWithoutReview_WhenReserving_ShouldBeApproved()
    {
        var schedule = CreateSchedule(requiresReview: false);

        var reservation = schedule.Reserve(Booker, Slot(10, 12), Now);

        reservation.Status.ShouldBe(ReservationStatus.Approved);
    }

    [Fact]
    public void GivenPolicyWithReview_WhenReserving_ShouldBePendingReview()
    {
        var schedule = CreateSchedule(requiresReview: true);

        var reservation = schedule.Reserve(Booker, Slot(10, 12), Now);

        reservation.Status.ShouldBe(ReservationStatus.PendingReview);
    }

    [Fact]
    public void GivenPolicyDeniesBooking_WhenReserving_ShouldThrowAndNotAddReservation()
    {
        var schedule = CreateSchedule();

        Should.Throw<InvalidOperationException>(() => schedule.Reserve(AssociationBooker, Slot(10, 12), Now));
        schedule.Reservations.ShouldBeEmpty();
    }

    [Fact]
    public void GivenAlreadyReservedTimeslot_WhenReservingSameTimeslot_ShouldThrow()
    {
        var schedule = CreateSchedule();
        schedule.Reserve(Booker, Slot(10, 12), Now);

        Should.Throw<InvalidOperationException>(() => schedule.Reserve(Booker, Slot(10, 12), Now));
        schedule.Reservations.Count.ShouldBe(1);
    }

    [Fact]
    public void GivenExistingReservation_WhenReservingOverlappingTimeslot_ShouldThrow()
    {
        var schedule = CreateSchedule();
        schedule.Reserve(Booker, Slot(10, 12), Now);

        Should.Throw<InvalidOperationException>(() => schedule.Reserve(Booker, Slot(11, 13), Now));
    }

    [Fact]
    public void GivenPendingReservation_WhenReservingOverlappingTimeslot_ShouldThrow()
    {
        var schedule = CreateSchedule(requiresReview: true);
        schedule.Reserve(Booker, Slot(10, 12), Now);

        Should.Throw<InvalidOperationException>(() => schedule.Reserve(Booker, Slot(11, 13), Now));
    }

    [Fact]
    public void GivenExistingReservation_WhenReservingAdjacentTimeslot_ShouldCreateReservation()
    {
        var schedule = CreateSchedule();
        schedule.Reserve(Booker, Slot(10, 12), Now);

        var reservation = schedule.Reserve(Booker, Slot(12, 14), Now);

        schedule.Reservations.Count.ShouldBe(2);
        schedule.Reservations.ShouldContain(reservation);
    }

    [Fact]
    public void GivenRejectedReservation_WhenReservingOverlappingTimeslot_ShouldCreateReservation()
    {
        var schedule = CreateSchedule(requiresReview: true);
        var rejected = schedule.Reserve(Booker, Slot(10, 12), Now);
        rejected.Reject(Approver, "Lokalen renoveras");

        var reservation = schedule.Reserve(Booker, Slot(10, 12), Now);

        reservation.Status.ShouldBe(ReservationStatus.PendingReview);
    }
}
