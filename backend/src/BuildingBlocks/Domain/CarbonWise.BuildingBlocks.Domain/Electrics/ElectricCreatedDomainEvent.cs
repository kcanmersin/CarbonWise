namespace CarbonWise.BuildingBlocks.Domain.Electrics
{
    public class ElectricCreatedDomainEvent : DomainEventBase
    {
        public ElectricId ElectricId { get; }

        public ElectricCreatedDomainEvent(ElectricId electricId)
        {
            ElectricId = electricId;
        }
    }
}