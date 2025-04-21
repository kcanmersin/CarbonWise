using System;
using CarbonWise.BuildingBlocks.Domain;

namespace CarbonWise.BuildingBlocks.Domain.SchoolInfos
{
    public class CampusVehicleEntry : Entity
    {
        public CampusVehicleEntryId Id { get; private set; }
        public int CarsManagedByUniversity { get; private set; }
        public int CarsEnteringUniversity { get; private set; }
        public int MotorcyclesEnteringUniversity { get; private set; }

        protected CampusVehicleEntry() { }

        private CampusVehicleEntry(
            CampusVehicleEntryId id,
            int carsManagedByUniversity,
            int carsEnteringUniversity,
            int motorcyclesEnteringUniversity)
        {
            Id = id;
            CarsManagedByUniversity = carsManagedByUniversity;
            CarsEnteringUniversity = carsEnteringUniversity;
            MotorcyclesEnteringUniversity = motorcyclesEnteringUniversity;
        }

        public static CampusVehicleEntry Create(
            int carsManagedByUniversity,
            int carsEnteringUniversity,
            int motorcyclesEnteringUniversity)
        {
            if (carsManagedByUniversity < 0)
                throw new ArgumentException("Cars managed by university cannot be negative", nameof(carsManagedByUniversity));

            if (carsEnteringUniversity < 0)
                throw new ArgumentException("Cars entering university cannot be negative", nameof(carsEnteringUniversity));

            if (motorcyclesEnteringUniversity < 0)
                throw new ArgumentException("Motorcycles entering university cannot be negative", nameof(motorcyclesEnteringUniversity));

            return new CampusVehicleEntry(
                new CampusVehicleEntryId(Guid.NewGuid()),
                carsManagedByUniversity,
                carsEnteringUniversity,
                motorcyclesEnteringUniversity);
        }

        public void UpdateCarsManagedByUniversity(int carsManagedByUniversity)
        {
            if (carsManagedByUniversity < 0)
                throw new ArgumentException("Cars managed by university cannot be negative", nameof(carsManagedByUniversity));

            CarsManagedByUniversity = carsManagedByUniversity;
        }

        public void UpdateCarsEnteringUniversity(int carsEnteringUniversity)
        {
            if (carsEnteringUniversity < 0)
                throw new ArgumentException("Cars entering university cannot be negative", nameof(carsEnteringUniversity));

            CarsEnteringUniversity = carsEnteringUniversity;
        }

        public void UpdateMotorcyclesEnteringUniversity(int motorcyclesEnteringUniversity)
        {
            if (motorcyclesEnteringUniversity < 0)
                throw new ArgumentException("Motorcycles entering university cannot be negative", nameof(motorcyclesEnteringUniversity));

            MotorcyclesEnteringUniversity = motorcyclesEnteringUniversity;
        }
    }
}