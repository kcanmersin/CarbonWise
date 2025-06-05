using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonWise.BuildingBlocks.Domain.AirQuality
{
    public class AirQualityCreatedDomainEvent : DomainEventBase
    {
        public AirQualityId AirQualityId { get; }

        public AirQualityCreatedDomainEvent(AirQualityId airQualityId)
        {
            AirQualityId = airQualityId;
        }
    }
}
