namespace TE.DomainStorytellingApplied.Domain.Tests;

public class BookingPolicyTests
{
    private static readonly DateTime Now = new(2030, 1, 1, 8, 0, 0);
    private static readonly TimeOnly OpensAt = new(8, 0);
    private static readonly TimeOnly ClosesAt = new(22, 0);

    private static readonly Booker PrivateBooker =
        new("Anna Andersson", "anna@example.com", BookerType.PrivateIndividual);

    private static readonly Booker AssociationBooker =
        new("Cecilia Carlsson", "cecilia@example.com", BookerType.AssociationRepresentative);

    private static BookingPolicy CreatePolicy(
        BookerType[]? allowedBookerTypes = null,
        bool requiresReview = false) =>
        new(
            allowedBookerTypes ?? [BookerType.PrivateIndividual],
            requiresReview,
            minDuration: TimeSpan.FromHours(1),
            maxDuration: TimeSpan.FromHours(4),
            opensAt: OpensAt,
            closesAt: ClosesAt);

    private static Timeslot Slot(int startHour, int endHour) =>
        new(new DateTime(2030, 1, 2, startHour, 0, 0), new DateTime(2030, 1, 2, endHour, 0, 0));

    [Fact]
    public void GivenValidValues_WhenCreatingPolicy_ShouldSetProperties()
    {
        var policy = CreatePolicy(requiresReview: true);

        policy.AllowedBookerTypes.ShouldBe([BookerType.PrivateIndividual]);
        policy.RequiresReview.ShouldBeTrue();
        policy.MinDuration.ShouldBe(TimeSpan.FromHours(1));
        policy.MaxDuration.ShouldBe(TimeSpan.FromHours(4));
        policy.OpensAt.ShouldBe(OpensAt);
        policy.ClosesAt.ShouldBe(ClosesAt);
    }

    [Fact]
    public void GivenMinDurationGreaterThanMaxDuration_WhenCreatingPolicy_ShouldThrow()
    {
        Should.Throw<ArgumentException>(() => new BookingPolicy(
            [BookerType.PrivateIndividual],
            requiresReview: false,
            minDuration: TimeSpan.FromHours(5),
            maxDuration: TimeSpan.FromHours(4),
            opensAt: OpensAt,
            closesAt: ClosesAt));
    }

    [Fact]
    public void GivenOpensAtNotBeforeClosesAt_WhenCreatingPolicy_ShouldThrow()
    {
        Should.Throw<ArgumentException>(() => new BookingPolicy(
            [BookerType.PrivateIndividual],
            requiresReview: false,
            minDuration: TimeSpan.FromHours(1),
            maxDuration: TimeSpan.FromHours(4),
            opensAt: ClosesAt,
            closesAt: OpensAt));
    }

    [Fact]
    public void GivenNoAllowedBookerTypes_WhenCreatingPolicy_ShouldThrow()
    {
        Should.Throw<ArgumentException>(() => CreatePolicy(allowedBookerTypes: []));
    }

    [Fact]
    public void GivenAllowedBookerAndValidTimeslot_WhenCheckingPolicy_ShouldAllow()
    {
        var policy = CreatePolicy();

        policy.Allows(PrivateBooker, Slot(10, 12), Now).ShouldBeTrue();
    }

    [Fact]
    public void GivenBookerTypeNotAllowed_WhenCheckingPolicy_ShouldDeny()
    {
        var policy = CreatePolicy();

        policy.Allows(AssociationBooker, Slot(10, 12), Now).ShouldBeFalse();
    }

    [Fact]
    public void GivenTimeslotShorterThanMinDuration_WhenCheckingPolicy_ShouldDeny()
    {
        var policy = CreatePolicy();
        var timeslot = new Timeslot(new DateTime(2030, 1, 2, 10, 0, 0), new DateTime(2030, 1, 2, 10, 30, 0));

        policy.Allows(PrivateBooker, timeslot, Now).ShouldBeFalse();
    }

    [Fact]
    public void GivenTimeslotLongerThanMaxDuration_WhenCheckingPolicy_ShouldDeny()
    {
        var policy = CreatePolicy();

        policy.Allows(PrivateBooker, Slot(10, 15), Now).ShouldBeFalse();
    }

    [Fact]
    public void GivenTimeslotExactlyMinAndMaxDuration_WhenCheckingPolicy_ShouldAllow()
    {
        var policy = CreatePolicy();

        policy.Allows(PrivateBooker, Slot(10, 11), Now).ShouldBeTrue();
        policy.Allows(PrivateBooker, Slot(10, 14), Now).ShouldBeTrue();
    }

    [Fact]
    public void GivenTimeslotStartingBeforeOpeningHours_WhenCheckingPolicy_ShouldDeny()
    {
        var policy = CreatePolicy();

        policy.Allows(PrivateBooker, Slot(7, 9), Now).ShouldBeFalse();
    }

    [Fact]
    public void GivenTimeslotEndingAfterClosingHours_WhenCheckingPolicy_ShouldDeny()
    {
        var policy = CreatePolicy();

        policy.Allows(PrivateBooker, Slot(21, 23), Now).ShouldBeFalse();
    }

    [Fact]
    public void GivenTimeslotInThePast_WhenCheckingPolicy_ShouldDeny()
    {
        var policy = CreatePolicy();
        var now = new DateTime(2030, 1, 3, 8, 0, 0);

        policy.Allows(PrivateBooker, Slot(10, 12), now).ShouldBeFalse();
    }
}
