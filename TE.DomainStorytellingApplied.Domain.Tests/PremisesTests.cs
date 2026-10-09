namespace TE.DomainStorytellingApplied.Domain.Tests;

public class PremisesTests
{
    private static readonly BookingPolicy Policy = new(
        [BookerType.PrivateIndividual],
        requiresReview: false,
        minDuration: TimeSpan.FromHours(1),
        maxDuration: TimeSpan.FromHours(4),
        opensAt: new TimeOnly(8, 0),
        closesAt: new TimeOnly(22, 0));

    [Fact]
    public void GivenValidValues_WhenCreatingPremises_ShouldSetProperties()
    {
        var premises = new Premises("Stora salen", 50, Policy);

        premises.Id.ShouldNotBe(Guid.Empty);
        premises.Name.ShouldBe("Stora salen");
        premises.Capacity.ShouldBe(50);
        premises.BookingPolicy.ShouldBe(Policy);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void GivenEmptyName_WhenCreatingPremises_ShouldThrow(string name)
    {
        Should.Throw<ArgumentException>(() => new Premises(name, 50, Policy));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void GivenCapacityNotGreaterThanZero_WhenCreatingPremises_ShouldThrow(int capacity)
    {
        Should.Throw<ArgumentException>(() => new Premises("Stora salen", capacity, Policy));
    }

    [Fact]
    public void GivenNoBookingPolicy_WhenCreatingPremises_ShouldThrow()
    {
        Should.Throw<ArgumentNullException>(() => new Premises("Stora salen", 50, null!));
    }
}
