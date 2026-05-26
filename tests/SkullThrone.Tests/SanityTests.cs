namespace SkullThrone.Tests;

using Xunit;

public class SanityTests
{
    [Fact]
    public void LogicalResolution_IsCorrect()
    {
        Assert.Equal(320, SkullThroneGame.LogicalWidth);
        Assert.Equal(200, SkullThroneGame.LogicalHeight);
    }
}
