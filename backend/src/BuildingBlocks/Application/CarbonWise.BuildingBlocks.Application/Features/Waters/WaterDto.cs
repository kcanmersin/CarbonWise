
namespace CarbonWise.BuildingBlocks.Application.Features.Waters
{
    public class WaterDto
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public decimal InitialMeterValue { get; set; }
        public decimal FinalMeterValue { get; set; }
        public decimal Usage { get; set; }
    }
}