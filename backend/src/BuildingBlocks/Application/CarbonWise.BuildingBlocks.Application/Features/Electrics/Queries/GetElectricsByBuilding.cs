using CarbonWise.BuildingBlocks.Domain.Buildings;
using CarbonWise.BuildingBlocks.Domain.Electrics;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonWise.BuildingBlocks.Application.Features.Electrics.Queries
{
        public class GetElectricsByBuildingQuery : IRequest<List<ElectricDto>>
        {
            public Guid BuildingId { get; set; }
        }


        public class GetElectricsByBuildingQueryHandler : IRequestHandler<GetElectricsByBuildingQuery, List<ElectricDto>>
        {
            private readonly IElectricRepository _electricRepository;
            private readonly IBuildingRepository _buildingRepository;

            public GetElectricsByBuildingQueryHandler(
                IElectricRepository electricRepository,
                IBuildingRepository buildingRepository)
            {
                _electricRepository = electricRepository;
                _buildingRepository = buildingRepository;
            }

            public async Task<List<ElectricDto>> Handle(GetElectricsByBuildingQuery request, CancellationToken cancellationToken)
            {
                var building = await _buildingRepository.GetByIdAsync(new BuildingId(request.BuildingId));
                if (building == null)
                {
                    throw new ApplicationException($"Building with id {request.BuildingId} not found");
                }

                var electrics = await _electricRepository.GetByBuildingIdAsync(new BuildingId(request.BuildingId));

                return electrics.Select(e => new ElectricDto
                {
                    Id = e.Id.Value,
                    Date = e.Date,
                    InitialMeterValue = e.InitialMeterValue,
                    FinalMeterValue = e.FinalMeterValue,
                    Usage = e.Usage,
                    KWHValue = e.KWHValue,
                    BuildingId = e.BuildingId.Value,
                    BuildingName = e.Building?.Name
                }).ToList();
            }
        }
}
