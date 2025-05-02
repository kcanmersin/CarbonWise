using CarbonWise.BuildingBlocks.Domain.Buildings;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonWise.BuildingBlocks.Application.Features.Buildings.Queries
{
    public class GetBuildingByIdQuery : IRequest<BuildingDto>
    {
        public Guid Id { get; set; }
    }

    public class GetBuildingByIdQueryHandler : IRequestHandler<GetBuildingByIdQuery, BuildingDto>
    {
        private readonly IBuildingRepository _buildingRepository;

        public GetBuildingByIdQueryHandler(IBuildingRepository buildingRepository)
        {
            _buildingRepository = buildingRepository;
        }

        public async Task<BuildingDto> Handle(GetBuildingByIdQuery request, CancellationToken cancellationToken)
        {
            var building = await _buildingRepository.GetByIdAsync(new BuildingId(request.Id));
            if (building == null)
            {
                return null;
            }

            return new BuildingDto
            {
                Id = building.Id.Value,
                Name = building.Name,
                E_MeterCode = building.E_MeterCode,
                G_MeterCode = building.G_MeterCode
            };
        }
    }
}
