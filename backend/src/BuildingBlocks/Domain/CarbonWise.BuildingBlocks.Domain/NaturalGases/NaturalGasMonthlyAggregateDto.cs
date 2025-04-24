using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonWise.BuildingBlocks.Domain.NaturalGases
{
    public class NaturalGasMonthlyAggregateDto
    {
        public DateTime YearMonth { get; set; }
        public Guid BuildingId { get; set; }
        public string BuildingName { get; set; }
        public decimal TotalSM3Value { get; set; }
        public decimal TotalUsage { get; set; }

        public string FormattedYearMonth => YearMonth.ToString("MM/yyyy");
    }
}
