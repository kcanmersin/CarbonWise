using System;

namespace CarbonWise.BuildingBlocks.Domain.CarbonFootprints
{
    public class CarbonFootprint
    {
        public int Year { get; private set; }
        public decimal ElectricityEmission { get; private set; }
        public decimal ShuttleBusEmission { get; private set; }
        public decimal CarEmission { get; private set; }
        public decimal MotorcycleEmission { get; private set; }
        public decimal TotalEmission { get; private set; }

        private CarbonFootprint() { }

        public CarbonFootprint(
            int year,
            decimal electricityUsage,
            int shuttleBusCount,
            int shuttleBusTripsPerDay,
            decimal shuttleBusTravelDistancePerDay,
            int carsEnteringCount,
            decimal carTravelDistancePerDay,
            int motorcyclesEnteringCount,
            decimal motorcycleTravelDistancePerDay,
            decimal electricityFactor = 0.84m,
            decimal shuttleBusFactor = 0.01m,
            decimal carFactor = 0.02m,
            decimal motorcycleFactor = 0.01m)
        {
            Year = year;

            ElectricityEmission = (electricityUsage / 1000) * electricityFactor;

            const int workDaysPerYear = 261;
            ShuttleBusEmission = (shuttleBusCount * shuttleBusTripsPerDay * shuttleBusTravelDistancePerDay * workDaysPerYear / 100) * shuttleBusFactor;

            CarEmission = (carsEnteringCount * 2 * carTravelDistancePerDay * workDaysPerYear / 100) * carFactor;

            MotorcycleEmission = (motorcyclesEnteringCount * 2 * motorcycleTravelDistancePerDay * workDaysPerYear / 100) * motorcycleFactor;

            TotalEmission = ElectricityEmission + ShuttleBusEmission + CarEmission + MotorcycleEmission;
        }
    }
}