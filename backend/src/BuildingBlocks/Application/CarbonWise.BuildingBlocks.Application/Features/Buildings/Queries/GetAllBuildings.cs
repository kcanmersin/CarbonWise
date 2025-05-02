using CarbonWise.BuildingBlocks.Domain.Buildings;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonWise.BuildingBlocks.Application.Features.Buildings.Queries
{
    public class GetAllBuildingsQuery : IRequest<List<BuildingDto>>
    {
    }

    public class GetAllBuildingsQueryHandler : IRequestHandler<GetAllBuildingsQuery, List<BuildingDto>>
    {
        private readonly IBuildingRepository _buildingRepository;

        public GetAllBuildingsQueryHandler(IBuildingRepository buildingRepository)
        {
            _buildingRepository = buildingRepository;
        }

        public async Task<List<BuildingDto>> Handle(GetAllBuildingsQuery request, CancellationToken cancellationToken)
        {
            var buildings = await _buildingRepository.GetAllAsync();

            return buildings.Select(b => new BuildingDto
            {
                Id = b.Id.Value,
                Name = b.Name,
                E_MeterCode = b.E_MeterCode,
                G_MeterCode = b.G_MeterCode
            }).ToList();
        }
    }
}
