namespace CarbonWise.BuildingBlocks.Domain.Waters
{
    public class WaterCreatedDomainEvent : DomainEventBase
    {
        public WaterId WaterId { get; }

        public WaterCreatedDomainEvent(WaterId waterId)
        {
            WaterId = waterId;
        }
    }
}

