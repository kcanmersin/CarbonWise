using System;
using CarbonWise.BuildingBlocks.Domain;

namespace CarbonWise.BuildingBlocks.Domain.SchoolInfos
{
    public class SchoolInfo : Entity, IAggregateRoot
    {
        public SchoolInfoId Id { get; private set; }
        public int NumberOfPeople { get; private set; }
        public CampusVehicleEntryId CampusVehicleEntryId { get; private set; }
        public CampusVehicleEntry Vehicles { get; private set; }
        public int Year { get; private set; }

        protected SchoolInfo() { }

        private SchoolInfo(SchoolInfoId id, int numberOfPeople, int year)
        {
            Id = id;
            NumberOfPeople = numberOfPeople;
            Year = year;
        }

        public static SchoolInfo Create(int numberOfPeople, int year)
        {
            if (numberOfPeople < 0)
                throw new ArgumentException("Number of people cannot be negative", nameof(numberOfPeople));

            if (year < 2000)
                throw new ArgumentException("Year must be 2000 or later", nameof(year));

            var schoolInfo = new SchoolInfo(new SchoolInfoId(Guid.NewGuid()), numberOfPeople, year);

            schoolInfo.AddDomainEvent(new SchoolInfoCreatedDomainEvent(schoolInfo.Id));

            return schoolInfo;
        }

        public void UpdateNumberOfPeople(int numberOfPeople)
        {
            if (numberOfPeople < 0)
                throw new ArgumentException("Number of people cannot be negative", nameof(numberOfPeople));

            NumberOfPeople = numberOfPeople;
        }

        public void AssignVehicleEntry(CampusVehicleEntry vehicleEntry)
        {
            if (vehicleEntry == null)
                throw new ArgumentNullException(nameof(vehicleEntry));

            Vehicles = vehicleEntry;
            CampusVehicleEntryId = vehicleEntry.Id;
        }
    }
}