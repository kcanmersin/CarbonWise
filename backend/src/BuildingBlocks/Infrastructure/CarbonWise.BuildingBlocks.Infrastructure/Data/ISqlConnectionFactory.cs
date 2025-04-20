using System.Data;

namespace CarbonWise.BuildingBlocks.Infrastructure.Data
{
    public interface ISqlConnectionFactory
    {
        IDbConnection GetOpenConnection();
    }
}