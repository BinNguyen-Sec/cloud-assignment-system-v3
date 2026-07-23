using CloudAssignment.Application.Common.Models;

namespace CloudAssignment.UnitTests.Common.Models;

public sealed class PageRequestTests
{
    [Fact]
    public void ConstructorWithDefaultsCreatesExpectedRequest()
    {
        var request = new PageRequest();

        Assert.Equal(1, request.Page);
        Assert.Equal(20, request.PageSize);
        Assert.Equal(0, request.Skip);
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(-1, 20)]
    [InlineData(1, 0)]
    [InlineData(1, 51)]
    public void ConstructorWithInvalidValuesThrows(
        int page,
        int pageSize)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new PageRequest(page, pageSize));
    }

    [Fact]
    public void SkipOnThirdPageIsCalculatedCorrectly()
    {
        var request = new PageRequest(
            page: 3,
            pageSize: 20);

        Assert.Equal(40, request.Skip);
    }
}
