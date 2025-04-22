namespace CarbonWise.BuildingBlocks.Domain.NaturalGases
{
    public class NaturalGasCreatedDomainEvent : DomainEventBase
    {
        public NaturalGasId NaturalGasId { get; }

        public NaturalGasCreatedDomainEvent(NaturalGasId naturalGasId)
        {
            NaturalGasId = naturalGasId;
        }
    }
}

