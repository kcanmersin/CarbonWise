using System;
using System.IO;
using System.Threading.Tasks;

namespace CarbonWise.BuildingBlocks.Application.Services.Reports
{
    public interface IPdfReportService
    {
        Task<byte[]> GenerateCarbonFootprintPdfReportAsync(DateTime startDate, DateTime endDate);

        Task<byte[]> GenerateConsumptionPdfReportAsync(string consumptionType, Guid? buildingId, DateTime startDate, DateTime endDate);
    }
}