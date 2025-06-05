using System;

namespace CarbonWise.BuildingBlocks.Domain.Papers
{
    public class PaperMonthlyTotalDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal TotalUsage { get; set; }

        public string FormattedMonth => $"{Month:D2}/{Year}";
        public DateTime MonthDate => new DateTime(Year, Month, 1);
    }

    public class PaperMonthlyAggregateDto
    {
        public DateTime YearMonth { get; set; }
        public Guid BuildingId { get; set; }
        public string BuildingName { get; set; }
        public decimal TotalUsage { get; set; }

        public string FormattedYearMonth => YearMonth.ToString("MM/yyyy");
    }
}