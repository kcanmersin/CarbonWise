using System;
using System.IO;
using System.Threading.Tasks;

namespace CarbonWise.BuildingBlocks.Application.Services.Reports
{
    public interface IPdfReportService
    {
        /// <summary>
        /// Generates a PDF report for carbon footprint data
        /// </summary>
        /// <param name="startDate">Start date for the report</param>
        /// <param name="endDate">End date for the report</param>
        /// <returns>PDF file as byte array</returns>
        Task<byte[]> GenerateCarbonFootprintPdfReportAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Generates a PDF report for consumption data
        /// </summary>
        /// <param name="consumptionType">Type of consumption (Electric, NaturalGas, Water, Paper)</param>
        /// <param name="buildingId">Building ID (nullable for reports across all buildings)</param>
        /// <param name="startDate">Start date for the report</param>
        /// <param name="endDate">End date for the report</param>
        /// <returns>PDF file as byte array</returns>
        Task<byte[]> GenerateConsumptionPdfReportAsync(string consumptionType, Guid? buildingId, DateTime startDate, DateTime endDate);
    }
}