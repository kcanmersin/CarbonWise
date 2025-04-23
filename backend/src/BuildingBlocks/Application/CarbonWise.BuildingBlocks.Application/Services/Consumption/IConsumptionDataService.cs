using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarbonWise.BuildingBlocks.Application.Services.Consumption
{
    public interface IConsumptionDataService
    {

        Task<IEnumerable<ConsumptionDataDto>> GetConsumptionDataAsync(
            string consumptionType,
            DateTime? startDate = null,
            DateTime? endDate = null);


        Task<byte[]> GenerateConsumptionExcelAsync(
            string consumptionType,
            DateTime? startDate = null,
            DateTime? endDate = null,
            bool includeGraphs = false);
    }
}