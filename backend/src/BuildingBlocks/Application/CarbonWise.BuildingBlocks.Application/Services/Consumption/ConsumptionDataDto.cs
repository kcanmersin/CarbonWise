using System;

namespace CarbonWise.BuildingBlocks.Application.Services.Consumption
{
    public class ConsumptionDataDto
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public decimal InitialMeterValue { get; set; }
        public decimal FinalMeterValue { get; set; }
        public decimal Usage { get; set; }
        public decimal? KWHValue { get; set; } // For Electric
        public decimal? SM3Value { get; set; } // For NaturalGas
        public Guid? BuildingId { get; set; }
        public string BuildingName { get; set; }
    }
}