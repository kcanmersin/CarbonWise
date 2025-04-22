using CarbonWise.BuildingBlocks.Domain.CarbonFootprints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonWise.BuildingBlocks.Application.Services.CarbonFootprints
{
    public interface ICarbonFootprintService
    {
        Task<CarbonFootprint> CalculateForYearAsync(int year);
        Task<List<CarbonFootprint>> CalculateForPeriodAsync(DateTime startDate, DateTime endDate);
    }
}
