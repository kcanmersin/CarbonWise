using CarbonWise.BuildingBlocks.Domain.CarbonFootprints;
using CarbonWise.BuildingBlocks.Domain.Electrics;
using CarbonWise.BuildingBlocks.Domain.SchoolInfos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonWise.BuildingBlocks.Application.Services.CarbonFootprints
{
    public class CarbonFootprintService : ICarbonFootprintService
    {
        private readonly IElectricRepository _electricRepository;
        private readonly ISchoolInfoRepository _schoolInfoRepository;

        public CarbonFootprintService(
            IElectricRepository electricRepository,
            ISchoolInfoRepository schoolInfoRepository)
        {
            _electricRepository = electricRepository;
            _schoolInfoRepository = schoolInfoRepository;
        }

        public async Task<CarbonFootprint> CalculateForYearAsync(
            int year,
            decimal? electricityFactor = null,
            decimal? shuttleBusFactor = null,
            decimal? carFactor = null,
            decimal? motorcycleFactor = null)
        {
            var startDate = new DateTime(year, 1, 1);
            var endDate = new DateTime(year, 12, 31);

            var electrics = await _electricRepository.GetByDateRangeAsync(startDate, endDate);
            decimal totalElectricityUsage = electrics.Sum(e => e.Usage);

            var schoolInfo = await _schoolInfoRepository.GetByYearAsync(year);

            if (schoolInfo == null)
            {
                throw new ApplicationException($"School information for year {year} not found");
            }

            int shuttleBusCount = 8;
            int shuttleBusTripsPerDay = 5;
            decimal shuttleBusTravelDistancePerDay = 5;

            int carsEnteringCount = schoolInfo.Vehicles?.CarsEnteringUniversity ?? 0;
            decimal carTravelDistancePerDay = 1.5m;

            int motorcyclesEnteringCount = schoolInfo.Vehicles?.MotorcyclesEnteringUniversity ?? 0;
            decimal motorcycleTravelDistancePerDay = 6;

            return new CarbonFootprint(
                year,
                totalElectricityUsage,
                shuttleBusCount,
                shuttleBusTripsPerDay,
                shuttleBusTravelDistancePerDay,
                carsEnteringCount,
                carTravelDistancePerDay,
                motorcyclesEnteringCount,
                motorcycleTravelDistancePerDay,
                electricityFactor ?? 0.84m,
                shuttleBusFactor ?? 0.01m,
                carFactor ?? 0.02m,
                motorcycleFactor ?? 0.01m);
        }

        public async Task<List<CarbonFootprint>> CalculateForPeriodAsync(
            DateTime startDate,
            DateTime endDate,
            decimal? electricityFactor = null,
            decimal? shuttleBusFactor = null,
            decimal? carFactor = null,
            decimal? motorcycleFactor = null)
        {
            var startYear = startDate.Year;
            var endYear = endDate.Year;

            var results = new List<CarbonFootprint>();

            for (int year = startYear; year <= endYear; year++)
            {
                try
                {
                    var carbonFootprint = await CalculateForYearAsync(
                        year,
                        electricityFactor,
                        shuttleBusFactor,
                        carFactor,
                        motorcycleFactor);
                    results.Add(carbonFootprint);
                }
                catch (ApplicationException)
                {
                    continue;
                }
            }

            return results;
        }

        public async Task<CarbonFootprintComparisonDto> GetYearComparisonAsync(
            decimal? electricityFactor = null,
            decimal? shuttleBusFactor = null,
            decimal? carFactor = null,
            decimal? motorcycleFactor = null)
        {
            var currentYear = DateTime.Now.Year;
            var previousYear = currentYear - 1;

            CarbonFootprint currentYearData = null;
            CarbonFootprint previousYearData = null;

            try
            {
                currentYearData = await CalculateForYearAsync(currentYear, electricityFactor, shuttleBusFactor, carFactor, motorcycleFactor);
            }
            catch (ApplicationException)
            {
                // Current year data might not be available
            }

            try
            {
                previousYearData = await CalculateForYearAsync(previousYear, electricityFactor, shuttleBusFactor, carFactor, motorcycleFactor);
            }
            catch (ApplicationException)
            {
                // Previous year data might not be available
            }

            return new CarbonFootprintComparisonDto
            {
                CurrentYear = currentYear,
                PreviousYear = previousYear,
                CurrentYearData = currentYearData != null ? new CarbonFootprintSummaryDto
                {
                    Year = currentYearData.Year,
                    ElectricityEmission = currentYearData.ElectricityEmission,
                    ShuttleBusEmission = currentYearData.ShuttleBusEmission,
                    CarEmission = currentYearData.CarEmission,
                    MotorcycleEmission = currentYearData.MotorcycleEmission,
                    TotalEmission = currentYearData.TotalEmission
                } : null,
                PreviousYearData = previousYearData != null ? new CarbonFootprintSummaryDto
                {
                    Year = previousYearData.Year,
                    ElectricityEmission = previousYearData.ElectricityEmission,
                    ShuttleBusEmission = previousYearData.ShuttleBusEmission,
                    CarEmission = previousYearData.CarEmission,
                    MotorcycleEmission = previousYearData.MotorcycleEmission,
                    TotalEmission = previousYearData.TotalEmission
                } : null,
                Comparison = CalculateComparison(currentYearData, previousYearData)
            };
        }

        private CarbonFootprintChangesDto CalculateComparison(CarbonFootprint current, CarbonFootprint previous)
        {
            if (current == null || previous == null)
            {
                return new CarbonFootprintChangesDto
                {
                    HasComparison = false,
                };
            }

            var changes = new CarbonFootprintChangesDto
            {
                HasComparison = true,
                TotalEmissionChange = current.TotalEmission - previous.TotalEmission,
                TotalEmissionChangePercentage = previous.TotalEmission != 0
                    ? Math.Round(((current.TotalEmission - previous.TotalEmission) / previous.TotalEmission) * 100, 2)
                    : 0,
                ElectricityEmissionChange = current.ElectricityEmission - previous.ElectricityEmission,
                ElectricityEmissionChangePercentage = previous.ElectricityEmission != 0
                    ? Math.Round(((current.ElectricityEmission - previous.ElectricityEmission) / previous.ElectricityEmission) * 100, 2)
                    : 0,
                ShuttleBusEmissionChange = current.ShuttleBusEmission - previous.ShuttleBusEmission,
                ShuttleBusEmissionChangePercentage = previous.ShuttleBusEmission != 0
                    ? Math.Round(((current.ShuttleBusEmission - previous.ShuttleBusEmission) / previous.ShuttleBusEmission) * 100, 2)
                    : 0,
                CarEmissionChange = current.CarEmission - previous.CarEmission,
                CarEmissionChangePercentage = previous.CarEmission != 0
                    ? Math.Round(((current.CarEmission - previous.CarEmission) / previous.CarEmission) * 100, 2)
                    : 0,
                MotorcycleEmissionChange = current.MotorcycleEmission - previous.MotorcycleEmission,
                MotorcycleEmissionChangePercentage = previous.MotorcycleEmission != 0
                    ? Math.Round(((current.MotorcycleEmission - previous.MotorcycleEmission) / previous.MotorcycleEmission) * 100, 2)
                    : 0,
                IsImprovement = current.TotalEmission < previous.TotalEmission,
            };

            return changes;
        }
    }

    public class CarbonFootprintComparisonDto
    {
        public int CurrentYear { get; set; }
        public int PreviousYear { get; set; }
        public CarbonFootprintSummaryDto CurrentYearData { get; set; }
        public CarbonFootprintSummaryDto PreviousYearData { get; set; }
        public CarbonFootprintChangesDto Comparison { get; set; }
    }

    public class CarbonFootprintSummaryDto
    {
        public int Year { get; set; }
        public decimal ElectricityEmission { get; set; }
        public decimal ShuttleBusEmission { get; set; }
        public decimal CarEmission { get; set; }
        public decimal MotorcycleEmission { get; set; }
        public decimal TotalEmission { get; set; }
    }

    public class CarbonFootprintChangesDto
    {
        public bool HasComparison { get; set; }
        public decimal TotalEmissionChange { get; set; }
        public decimal TotalEmissionChangePercentage { get; set; }
        public decimal ElectricityEmissionChange { get; set; }
        public decimal ElectricityEmissionChangePercentage { get; set; }
        public decimal ShuttleBusEmissionChange { get; set; }
        public decimal ShuttleBusEmissionChangePercentage { get; set; }
        public decimal CarEmissionChange { get; set; }
        public decimal CarEmissionChangePercentage { get; set; }
        public decimal MotorcycleEmissionChange { get; set; }
        public decimal MotorcycleEmissionChangePercentage { get; set; }
        public bool IsImprovement { get; set; }
    }
}