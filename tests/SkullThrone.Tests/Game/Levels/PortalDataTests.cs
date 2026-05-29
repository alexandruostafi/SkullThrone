namespace SkullThrone.Tests.Game.Levels;

using SkullThrone.Game.Levels;
using Xunit;

/// <summary>
/// Unit tests for <see cref="PortalData"/>.
/// ISTQB techniques: equivalence partitioning, boundary value analysis, error guessing.
/// </summary>
public sealed class PortalDataTests
{
    #region Constructor — Valid Inputs (EP: valid partition)

    [Fact]
    public void Constructor_ValidParameters_CreatesPortalData()
    {
        // Arrange & Act
        var portal = new PortalData(
            "portal_1",
            new PortalEndpoint { X = 5, Y = 5, Face = PortalFace.East },
            new PortalEndpoint { X = 10, Y = 10, Face = PortalFace.West },
            "green");

        // Assert
        Assert.Equal("portal_1", portal.Id);
        Assert.Equal(5, portal.TileA.X);
        Assert.Equal(5, portal.TileA.Y);
        Assert.Equal(PortalFace.East, portal.TileA.Face);
        Assert.Equal(10, portal.TileB.X);
        Assert.Equal(10, portal.TileB.Y);
        Assert.Equal(PortalFace.West, portal.TileB.Face);
        Assert.Equal("green", portal.Color);
    }

    #endregion

    #region Constructor — Invalid Inputs (EP: invalid partition / error guessing)

    [Fact]
    public void Constructor_NullId_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new PortalData(
            null!,
            new PortalEndpoint { X = 0, Y = 0, Face = PortalFace.North },
            new PortalEndpoint { X = 1, Y = 1, Face = PortalFace.South },
            "green"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_InvalidId_ThrowsArgumentException(string id)
    {
        Assert.Throws<ArgumentException>(() => new PortalData(
            id,
            new PortalEndpoint { X = 0, Y = 0, Face = PortalFace.North },
            new PortalEndpoint { X = 1, Y = 1, Face = PortalFace.South },
            "green"));
    }

    [Fact]
    public void Constructor_NullColor_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new PortalData(
            "portal_1",
            new PortalEndpoint { X = 0, Y = 0, Face = PortalFace.North },
            new PortalEndpoint { X = 1, Y = 1, Face = PortalFace.South },
            null!));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_InvalidColor_ThrowsArgumentException(string color)
    {
        Assert.Throws<ArgumentException>(() => new PortalData(
            "portal_1",
            new PortalEndpoint { X = 0, Y = 0, Face = PortalFace.North },
            new PortalEndpoint { X = 1, Y = 1, Face = PortalFace.South },
            color));
    }

    #endregion

    #region GetDestination — Decision Table

    // Decision table:
    // | Input (tileX, tileY) | Matches TileA | Matches TileB | Result    |
    // |----------------------|---------------|---------------|-----------|
    // | TileA coords         | Yes           | No            | TileB     |
    // | TileB coords         | No            | Yes           | TileA     |
    // | Neither              | No            | No            | null      |

    [Fact]
    public void GetDestination_MatchesTileA_ReturnsTileB()
    {
        var endpointA = new PortalEndpoint { X = 3, Y = 7, Face = PortalFace.East };
        var endpointB = new PortalEndpoint { X = 12, Y = 4, Face = PortalFace.West };
        var portal = new PortalData("p1", endpointA, endpointB, "purple");

        var result = portal.GetDestination(3, 7);

        Assert.NotNull(result);
        Assert.Equal(12, result.Value.X);
        Assert.Equal(4, result.Value.Y);
        Assert.Equal(PortalFace.West, result.Value.Face);
    }

    [Fact]
    public void GetDestination_MatchesTileB_ReturnsTileA()
    {
        var endpointA = new PortalEndpoint { X = 3, Y = 7, Face = PortalFace.East };
        var endpointB = new PortalEndpoint { X = 12, Y = 4, Face = PortalFace.West };
        var portal = new PortalData("p1", endpointA, endpointB, "purple");

        var result = portal.GetDestination(12, 4);

        Assert.NotNull(result);
        Assert.Equal(3, result.Value.X);
        Assert.Equal(7, result.Value.Y);
        Assert.Equal(PortalFace.East, result.Value.Face);
    }

    [Fact]
    public void GetDestination_MatchesNeither_ReturnsNull()
    {
        var portal = new PortalData(
            "p1",
            new PortalEndpoint { X = 3, Y = 7, Face = PortalFace.East },
            new PortalEndpoint { X = 12, Y = 4, Face = PortalFace.West },
            "green");

        var result = portal.GetDestination(99, 99);

        Assert.Null(result);
    }

    #endregion

    #region GetDestination — Boundary Value Analysis

    [Fact]
    public void GetDestination_SameCoordinatesForBothEndpoints_ReturnsSelf()
    {
        // Edge case: both endpoints at same tile (unusual but valid data)
        var endpoint = new PortalEndpoint { X = 5, Y = 5, Face = PortalFace.North };
        var portal = new PortalData("p1", endpoint, endpoint, "blue");

        // Should match TileA first and return TileB (which is the same)
        var result = portal.GetDestination(5, 5);

        Assert.NotNull(result);
        Assert.Equal(5, result.Value.X);
        Assert.Equal(5, result.Value.Y);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(int.MaxValue, int.MaxValue)]
    public void GetDestination_ExtremeCoordinates_ReturnsNull(int x, int y)
    {
        var portal = new PortalData(
            "p1",
            new PortalEndpoint { X = 5, Y = 5, Face = PortalFace.North },
            new PortalEndpoint { X = 10, Y = 10, Face = PortalFace.South },
            "green");

        var result = portal.GetDestination(x, y);

        Assert.Null(result);
    }

    #endregion
}
