namespace CarbonWise.BuildingBlocks.Application
{
    public interface IExecutionContextAccessor
    {
        Guid UserId { get; }
        string UserName { get; }
        bool IsAuthenticated { get; }
        string CorrelationId { get; }
    }
}