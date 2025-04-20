namespace CarbonWise.BuildingBlocks.Domain.Users
{
    public class UserCreatedDomainEvent : DomainEventBase
    {
        public UserId UserId { get; }

        public UserCreatedDomainEvent(UserId userId)
        {
            UserId = userId;
        }
    }
}