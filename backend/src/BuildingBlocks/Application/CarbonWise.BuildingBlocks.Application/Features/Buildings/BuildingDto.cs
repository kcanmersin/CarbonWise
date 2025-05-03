using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonWise.BuildingBlocks.Application.Features.Buildings
{
    public class BuildingDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string E_MeterCode { get; set; }
        public string G_MeterCode { get; set; }
    }
}
