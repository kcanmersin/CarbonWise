using CarbonWise.BuildingBlocks.Domain.CarbonFootprints;

public interface ICarbonFootprintService
{
    Task<CarbonFootprint> CalculateForYearAsync(
        int year,
        decimal? electricityFactor = null,
        decimal? shuttleBusFactor = null,
        decimal? carFactor = null,
        decimal? motorcycleFactor = null);

    Task<List<CarbonFootprint>> CalculateForPeriodAsync(
        DateTime startDate,
        DateTime endDate,
        decimal? electricityFactor = null,
        decimal? shuttleBusFactor = null,
        decimal? carFactor = null,
        decimal? motorcycleFactor = null);
}