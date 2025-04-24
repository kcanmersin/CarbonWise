using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonWise.BuildingBlocks.Domain.Electrics
{
    public class ElectricMonthlyTotalDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal TotalKWHValue { get; set; }
        public decimal TotalUsage { get; set; }

        public string FormattedMonth => $"{Month:D2}/{Year}";
        public DateTime MonthDate => new DateTime(Year, Month, 1);
    }
}
