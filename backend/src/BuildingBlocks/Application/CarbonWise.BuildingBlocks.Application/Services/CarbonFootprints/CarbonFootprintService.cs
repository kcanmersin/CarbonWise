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
    }
}
