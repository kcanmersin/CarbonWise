using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CarbonWise.BuildingBlocks.Domain.Buildings;
using CarbonWise.BuildingBlocks.Domain.Papers;
using MediatR;

namespace CarbonWise.BuildingBlocks.Application.Features.Papers.Queries.FilterPapers
{
    public class FilterPapersQuery : IRequest<List<PaperDto>>
    {
        public Guid? BuildingId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class FilterPapersQueryHandler : IRequestHandler<FilterPapersQuery, List<PaperDto>>
    {
        private readonly IPaperRepository _paperRepository;
        private readonly IBuildingRepository _buildingRepository;

        public FilterPapersQueryHandler(
            IPaperRepository paperRepository,
            IBuildingRepository buildingRepository)
        {
            _paperRepository = paperRepository;
            _buildingRepository = buildingRepository;
        }

        public async Task<List<PaperDto>> Handle(FilterPapersQuery request, CancellationToken cancellationToken)
        {
            if (request.BuildingId.HasValue)
            {
                var building = await _buildingRepository.GetByIdAsync(new BuildingId(request.BuildingId.Value));
                if (building == null)
                {
                    throw new ApplicationException($"Building with id {request.BuildingId} not found");
                }
            }

            List<Paper> papers;

            if (request.BuildingId.HasValue && request.StartDate.HasValue && request.EndDate.HasValue)
            {
                papers = await _paperRepository.GetByBuildingIdAndDateRangeAsync(
                    new BuildingId(request.BuildingId.Value),
                    request.StartDate.Value,
                    request.EndDate.Value);
            }
            else if (request.BuildingId.HasValue)
            {
                papers = await _paperRepository.GetByBuildingIdAsync(
                    new BuildingId(request.BuildingId.Value));
            }
            else if (request.StartDate.HasValue && request.EndDate.HasValue)
            {
                papers = await _paperRepository.GetByDateRangeAsync(
                    request.StartDate.Value,
                    request.EndDate.Value);
            }
            else
            {
                papers = await _paperRepository.GetAllAsync();
            }

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