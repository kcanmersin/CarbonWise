using System;
using CarbonWise.BuildingBlocks.Domain.Buildings;
using CarbonWise.BuildingBlocks.Domain;

namespace CarbonWise.BuildingBlocks.Domain.Electrics
{
    public class Electric : Entity, IAggregateRoot
    {
        public ElectricId Id { get; private set; }
        public DateTime Date { get; private set; }
        public decimal InitialMeterValue { get; private set; }
        public decimal FinalMeterValue { get; private set; }
        public decimal Usage { get; private set; }
        public decimal KWHValue { get; private set; }
        public BuildingId BuildingId { get; private set; }
        public virtual Building Building { get; private set; }

        protected Electric() { }

        private Electric(
            ElectricId id,
            DateTime date,
            decimal initialMeterValue,
            decimal finalMeterValue,
            decimal usage,
            decimal kwhValue,
            BuildingId buildingId)
        {
            Id = id;
            Date = date;
            InitialMeterValue = initialMeterValue;
            FinalMeterValue = finalMeterValue;
            Usage = usage;
            KWHValue = kwhValue;
            BuildingId = buildingId;
        }

        public static Electric Create(
            DateTime date,
            decimal initialMeterValue,
            decimal finalMeterValue,
            decimal kwhValue,
            BuildingId buildingId)
        {
            if (initialMeterValue < 0)
                throw new ArgumentException("Initial meter value cannot be negative", nameof(initialMeterValue));

            if (finalMeterValue < initialMeterValue)
                throw new ArgumentException("Final meter value cannot be less than initial meter value", nameof(finalMeterValue));

            if (kwhValue <= 0)
                throw new ArgumentException("KWH value must be positive", nameof(kwhValue));

            decimal usage = (finalMeterValue - initialMeterValue) * kwhValue;

            var electric = new Electric(
                new ElectricId(Guid.NewGuid()),
                date,
                initialMeterValue,
                finalMeterValue,
                usage,
                kwhValue,
                buildingId);

            electric.AddDomainEvent(new ElectricCreatedDomainEvent(electric.Id));

            return electric;
        }

        public void Update(
            DateTime date,
            decimal initialMeterValue,
            decimal finalMeterValue,
            decimal kwhValue)
        {
            if (initialMeterValue < 0)
                throw new ArgumentException("Initial meter value cannot be negative", nameof(initialMeterValue));

            if (finalMeterValue < initialMeterValue)
                throw new ArgumentException("Final meter value cannot be less than initial meter value", nameof(finalMeterValue));

            if (kwhValue <= 0)
                throw new ArgumentException("KWH value must be positive", nameof(kwhValue));

            Date = date;
            InitialMeterValue = initialMeterValue;
            FinalMeterValue = finalMeterValue;
            KWHValue = kwhValue;

            // Recalculate usage
            Usage = (finalMeterValue - initialMeterValue) * kwhValue;
        }
    }
}