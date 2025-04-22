using System;
using System.ComponentModel.DataAnnotations;
using CarbonWise.BuildingBlocks.Domain;

namespace CarbonWise.BuildingBlocks.Domain.Buildings
{
    public class Building : Entity, IAggregateRoot
    {
        public BuildingId Id { get; private set; }
        public string Name { get; private set; }
        public string? E_MeterCode { get; private set; }
        public string? G_MeterCode { get; private set; }

        protected Building() { }

        private Building(BuildingId id, string name, string eMeterCode, string gMeterCode)
        {
            Id = id;
            Name = name;
            E_MeterCode = eMeterCode;
            G_MeterCode = gMeterCode;
        }

        public static Building Create(string name, string eMeterCode = null, string gMeterCode = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Building name is required", nameof(name));

            if (name.Length > 100)
                throw new ArgumentException("Building name cannot exceed 100 characters", nameof(name));

            if (string.IsNullOrWhiteSpace(eMeterCode) && string.IsNullOrWhiteSpace(gMeterCode))
                throw new ArgumentException("At least one meter code (electricity or gas) must be provided");

            if (eMeterCode != null && eMeterCode.Length > 20)
                throw new ArgumentException("E_MeterCode cannot exceed 20 characters", nameof(eMeterCode));

            if (gMeterCode != null && gMeterCode.Length > 20)
                throw new ArgumentException("G_MeterCode cannot exceed 20 characters", nameof(gMeterCode));


            var building = new Building(
                new BuildingId(Guid.NewGuid()),
                name,
                eMeterCode,
                gMeterCode);

            building.AddDomainEvent(new BuildingCreatedDomainEvent(building.Id));

            return building;
        }

        public void Update(string name, string? eMeterCode = null, string? gMeterCode = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Building name is required", nameof(name));

            if (name.Length > 100)
                throw new ArgumentException("Building name cannot exceed 100 characters", nameof(name));

            if (string.IsNullOrWhiteSpace(eMeterCode) && string.IsNullOrWhiteSpace(gMeterCode))
                throw new ArgumentException("At least one meter code (electricity or gas) must be provided");

            if (eMeterCode != null && eMeterCode.Length > 20)
                throw new ArgumentException("E_MeterCode cannot exceed 20 characters", nameof(eMeterCode));

            if (gMeterCode != null && gMeterCode.Length > 20)
                throw new ArgumentException("G_MeterCode cannot exceed 20 characters", nameof(gMeterCode));

            Name = name;
            E_MeterCode = eMeterCode;
            G_MeterCode = gMeterCode;
        }
    }
}