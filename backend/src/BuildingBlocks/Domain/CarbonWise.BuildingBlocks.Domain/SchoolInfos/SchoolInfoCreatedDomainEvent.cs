using CarbonWise.BuildingBlocks.Domain.SchoolInfos;

namespace CarbonWise.BuildingBlocks.Domain.SchoolInfos
{
    public class SchoolInfoCreatedDomainEvent : DomainEventBase
    {
        public SchoolInfoId SchoolInfoId { get; }

        public SchoolInfoCreatedDomainEvent(SchoolInfoId schoolInfoId)
        {
            SchoolInfoId = schoolInfoId;
        }
    }
}