using System;

namespace CarbonWise.BuildingBlocks.Application.Features.Papers
{
    public class PaperDto
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public decimal Usage { get; set; }
        public Guid BuildingId { get; set; }
        public string BuildingName { get; set; }
    }
}