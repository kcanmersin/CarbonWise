namespace CarbonWise.BuildingBlocks.Domain.Buildings
{
    public class BuildingCreatedDomainEvent : DomainEventBase
    {
        public BuildingId BuildingId { get; }

        public BuildingCreatedDomainEvent(BuildingId buildingId)
        {
            BuildingId = buildingId;
        }
    }
}