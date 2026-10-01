namespace TE.DomainStorytellingApplied.Domain.Tests;

public class SanityTests
{
    [Fact]
    public void GivenMathIsNotBroken_WhenSumming1And2_ShouldBe3()
    {
        var a = 1;
        var b = 2;

        var sum = a + b;

        sum.ShouldBe(3);
    }
}