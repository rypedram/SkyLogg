using SkyLogg.Domain.Services;
using SkyLogg.Domain.ValueObjects;

namespace SkyLogg.Tests.Features.Domain;

[TestClass]
public class FlightCalculationServiceTests
{
    [TestMethod]
    public void CalculateSectorDuration_UsesTakeoffLandingWhenPresent()
    {
        var blockOff = new DateTimeOffset(2026, 1, 5, 8, 10, 0, TimeSpan.Zero);
        var blockOn = new DateTimeOffset(2026, 1, 5, 10, 30, 0, TimeSpan.Zero);
        var takeoff = new DateTimeOffset(2026, 1, 5, 8, 25, 0, TimeSpan.Zero);
        var landing = new DateTimeOffset(2026, 1, 5, 10, 15, 0, TimeSpan.Zero);

        var result = FlightCalculationService.CalculateSectorDuration(blockOff, blockOn, takeoff, landing);

        Assert.AreEqual(140, result.BlockMinutes);
        Assert.AreEqual(110, result.FlightMinutes);
    }

    [TestMethod]
    public void CalculateSectorDuration_FallsBackToBlockTime()
    {
        var blockOff = new DateTimeOffset(2026, 1, 5, 8, 0, 0, TimeSpan.Zero);
        var blockOn = new DateTimeOffset(2026, 1, 5, 9, 30, 0, TimeSpan.Zero);

        var result = FlightCalculationService.CalculateSectorDuration(blockOff, blockOn, null, null);

        Assert.AreEqual(90, result.BlockMinutes);
        Assert.AreEqual(90, result.FlightMinutes);
    }
}

[TestClass]
public class AirportCodeTests
{
    [TestMethod]
    public void Create_AcceptsValidIcao()
    {
        var code = AirportCode.Create("oiii");
        Assert.AreEqual("OIII", code.Value);
        Assert.IsTrue(code.IsIcao);
    }

    [TestMethod]
    public void Create_RejectsInvalidLength()
    {
        Assert.ThrowsExactly<SkyLogg.Domain.Exceptions.DomainValidationException>(() => AirportCode.Create("OI"));
    }
}
