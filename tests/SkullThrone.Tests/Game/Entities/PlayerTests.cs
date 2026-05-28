namespace SkullThrone.Tests.Game.Entities;

using System;
using SkullThrone.Game.Entities;
using Xunit;

/// <summary>
/// Unit tests for <see cref="Player"/>.
/// ISTQB techniques: equivalence partitioning, boundary value analysis.
/// </summary>
public sealed class PlayerTests
{
    #region Constructor

    [Fact]
    public void Constructor_ValidParameters_SetsProperties()
    {
        var player = new Player(5.5f, 3.2f, MathF.PI / 2f);

        Assert.Equal(5.5f, player.X);
        Assert.Equal(3.2f, player.Y);
        Assert.Equal(MathF.PI / 2f, player.Angle);
    }

    [Fact]
    public void Constructor_ZeroValues_Succeeds()
    {
        var player = new Player(0f, 0f, 0f);

        Assert.Equal(0f, player.X);
        Assert.Equal(0f, player.Y);
        Assert.Equal(0f, player.Angle);
    }

    [Fact]
    public void Constructor_NegativeAngle_Accepted()
    {
        var player = new Player(1f, 1f, -MathF.PI);
        Assert.Equal(-MathF.PI, player.Angle);
    }

    #endregion

    #region Property Mutation

    [Fact]
    public void Position_CanBeUpdated()
    {
        var player = new Player(1f, 1f, 0f);

        player.X = 10f;
        player.Y = 20f;

        Assert.Equal(10f, player.X);
        Assert.Equal(20f, player.Y);
    }

    [Fact]
    public void Angle_CanBeUpdated()
    {
        var player = new Player(1f, 1f, 0f);

        player.Angle = MathF.Tau; // Full rotation

        Assert.Equal(MathF.Tau, player.Angle);
    }

    #endregion

    #region EP — Extreme Float Values

    [Theory]
    [InlineData(float.MaxValue)]
    [InlineData(float.MinValue)]
    [InlineData(float.NaN)]
    [InlineData(float.PositiveInfinity)]
    [InlineData(float.NegativeInfinity)]
    public void Constructor_ExtremeFloatValues_DoesNotThrow(float value)
    {
        // Documents that Player has no validation — accepts any float
        var player = new Player(value, value, value);

        Assert.Equal(value, player.X);
        Assert.Equal(value, player.Y);
        Assert.Equal(value, player.Angle);
    }

    #endregion

    #region Pitch — Equivalence Partitions

    [Fact]
    public void Pitch_SetZero_ReturnsZero()
    {
        var player = new Player(1f, 1f, 0f);
        player.Pitch = 0;
        Assert.Equal(0, player.Pitch);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-40)]
    [InlineData(-80)]
    public void Pitch_SetValidNegative_ReturnsExactValue(int pitch)
    {
        var player = new Player(1f, 1f, 0f);
        player.Pitch = pitch;
        Assert.Equal(pitch, player.Pitch);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(40)]
    [InlineData(80)]
    public void Pitch_SetValidPositive_ReturnsExactValue(int pitch)
    {
        var player = new Player(1f, 1f, 0f);
        player.Pitch = pitch;
        Assert.Equal(pitch, player.Pitch);
    }

    [Theory]
    [InlineData(81, 80)]
    [InlineData(100, 80)]
    [InlineData(int.MaxValue, 80)]
    public void Pitch_SetAboveMax_ClampedToMaxPitch(int input, int expected)
    {
        var player = new Player(1f, 1f, 0f);
        player.Pitch = input;
        Assert.Equal(expected, player.Pitch);
    }

    [Theory]
    [InlineData(-81, -80)]
    [InlineData(-100, -80)]
    [InlineData(int.MinValue, -80)]
    public void Pitch_SetBelowMin_ClampedToNegativeMaxPitch(int input, int expected)
    {
        var player = new Player(1f, 1f, 0f);
        player.Pitch = input;
        Assert.Equal(expected, player.Pitch);
    }

    #endregion

    #region Pitch — Boundary Value Analysis

    [Theory]
    [InlineData(-80, -80)]
    [InlineData(-79, -79)]
    [InlineData(79, 79)]
    [InlineData(80, 80)]
    public void Pitch_BoundaryValues_ReturnsExpected(int input, int expected)
    {
        var player = new Player(1f, 1f, 0f);
        player.Pitch = input;
        Assert.Equal(expected, player.Pitch);
    }

    [Fact]
    public void Pitch_DefaultValue_IsZero()
    {
        var player = new Player(1f, 1f, 0f);
        Assert.Equal(0, player.Pitch);
    }

    [Fact]
    public void Pitch_SetMultipleTimes_LastValueWins()
    {
        var player = new Player(1f, 1f, 0f);
        player.Pitch = 50;
        player.Pitch = -30;
        Assert.Equal(-30, player.Pitch);
    }

    [Fact]
    public void MaxPitch_Constant_Equals80()
    {
        Assert.Equal(80, Player.MaxPitch);
    }

    #endregion
}
