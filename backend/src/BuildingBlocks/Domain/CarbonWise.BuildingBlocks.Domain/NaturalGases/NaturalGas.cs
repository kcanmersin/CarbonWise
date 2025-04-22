using System;
using CarbonWise.BuildingBlocks.Domain.Buildings;
using CarbonWise.BuildingBlocks.Domain;

namespace CarbonWise.BuildingBlocks.Domain.NaturalGases
{
    public class NaturalGas : Entity, IAggregateRoot
    {
        public NaturalGasId Id { get; private set; }
        public DateTime Date { get; private set; }
        public decimal InitialMeterValue { get; private set; }
        public decimal FinalMeterValue { get; private set; }
        public decimal Usage { get; private set; }
        public decimal SM3Value { get; private set; }
        public BuildingId BuildingId { get; private set; }
        public virtual Building Building { get; private set; }

        protected NaturalGas() { }

        private NaturalGas(
            NaturalGasId id,
            DateTime date,
            decimal initialMeterValue,
            decimal finalMeterValue,
            decimal usage,
            decimal sm3Value,
            BuildingId buildingId)
        {
            Id = id;
            Date = date;
            InitialMeterValue = initialMeterValue;
            FinalMeterValue = finalMeterValue;
            Usage = usage;
            SM3Value = sm3Value;
            BuildingId = buildingId;
        }

        public static NaturalGas Create(
            DateTime date,
            decimal initialMeterValue,
            decimal finalMeterValue,
            decimal sm3Value,
            BuildingId buildingId)
        {
            if (initialMeterValue < 0)
                throw new ArgumentException("Initial meter value cannot be negative", nameof(initialMeterValue));

            if (finalMeterValue < initialMeterValue)
                throw new ArgumentException("Final meter value cannot be less than initial meter value", nameof(finalMeterValue));

            if (sm3Value <= 0)
                throw new ArgumentException("SM3 value must be positive", nameof(sm3Value));

            decimal usage = (finalMeterValue - initialMeterValue) * sm3Value;

            var naturalGas = new NaturalGas(
                new NaturalGasId(Guid.NewGuid()),
                date,
                initialMeterValue,
                finalMeterValue,
                usage,
                sm3Value,
                buildingId);

            naturalGas.AddDomainEvent(new NaturalGasCreatedDomainEvent(naturalGas.Id));

            return naturalGas;
        }

        public void Update(
            DateTime date,
            decimal initialMeterValue,
            decimal finalMeterValue,
            decimal sm3Value)
        {
            if (initialMeterValue < 0)
                throw new ArgumentException("Initial meter value cannot be negative", nameof(initialMeterValue));

            if (finalMeterValue < initialMeterValue)
                throw new ArgumentException("Final meter value cannot be less than initial meter value", nameof(finalMeterValue));

            if (sm3Value <= 0)
                throw new ArgumentException("SM3 value must be positive", nameof(sm3Value));

            Date = date;
            InitialMeterValue = initialMeterValue;
            FinalMeterValue = finalMeterValue;
            SM3Value = sm3Value;

            Usage = (finalMeterValue - initialMeterValue) * sm3Value;
        }
    }
}



