using CarbonWise.BuildingBlocks.Domain.Buildings;
using CarbonWise.BuildingBlocks.Domain.Papers;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonWise.BuildingBlocks.Application.Features.Papers.Queries
{
    public class GetPapersByBuildingQuery : IRequest<List<PaperDto>>
    {
        public Guid BuildingId { get; set; }
    }

    public class GetPapersByBuildingQueryHandler : IRequestHandler<GetPapersByBuildingQuery, List<PaperDto>>
    {
        private readonly IPaperRepository _paperRepository;
        private readonly IBuildingRepository _buildingRepository;

        public GetPapersByBuildingQueryHandler(
            IPaperRepository paperRepository,
            IBuildingRepository buildingRepository)
        {
            _paperRepository = paperRepository;
            _buildingRepository = buildingRepository;
        }

        public async Task<List<PaperDto>> Handle(GetPapersByBuildingQuery request, CancellationToken cancellationToken)
        {
            var building = await _buildingRepository.GetByIdAsync(new BuildingId(request.BuildingId));
            if (building == null)
            {
                throw new ApplicationException($"Building with id {request.BuildingId} not found");
            }

            var papers = await _paperRepository.GetByBuildingIdAsync(new BuildingId(request.BuildingId));

            return papers.Select(p => new PaperDto
            {
                Id = p.Id.Value,
                Date = p.Date,
                Usage = p.Usage,
                BuildingId = p.BuildingId.Value,
                BuildingName = p.Building?.Name
            }).ToList();
        }
    }
}
