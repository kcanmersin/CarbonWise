using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarbonWise.BuildingBlocks.Application.Services.Reports
{
    public interface IReportService
    {
      
        Task<ReportDto> GenerateCarbonFootprintReportAsync(DateTime startDate, DateTime endDate);


        Task<ReportDto> GenerateConsumptionReportAsync(string consumptionType, Guid? buildingId, DateTime startDate, DateTime endDate);

        Task<List<string>> GetConsumptionTypesAsync();
    }

    public class ReportDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string ReportType { get; set; }
        public string ConsumptionType { get; set; } 
        public Guid? BuildingId { get; set; }
        public string BuildingName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime GeneratedAt { get; set; }
        public string Analysis { get; set; } 
        public object Data { get; set; } 
    }

    public class ReportRequest
    {
        public string ReportType { get; set; }
        public string ConsumptionType { get; set; } 
        public Guid? BuildingId { get; set; } 
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}