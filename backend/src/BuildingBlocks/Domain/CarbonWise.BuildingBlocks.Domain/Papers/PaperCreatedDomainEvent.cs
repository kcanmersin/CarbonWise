namespace CarbonWise.BuildingBlocks.Domain.Papers
{
    public class PaperCreatedDomainEvent : DomainEventBase
    {
        public PaperId PaperId { get; }

        public PaperCreatedDomainEvent(PaperId paperId)
        {
            PaperId = paperId;
        }
    }
}