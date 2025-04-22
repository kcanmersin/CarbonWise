using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CarbonWise.BuildingBlocks.Domain.Papers;
using MediatR;

namespace CarbonWise.BuildingBlocks.Application.Features.Papers.Queries.FilterPapers
{
    public class FilterPapersQuery : IRequest<List<PaperDto>>
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class FilterPapersQueryHandler : IRequestHandler<FilterPapersQuery, List<PaperDto>>
    {
        private readonly IPaperRepository _paperRepository;

        public FilterPapersQueryHandler(IPaperRepository paperRepository)
        {
            _paperRepository = paperRepository;
        }

        public async Task<List<PaperDto>> Handle(FilterPapersQuery request, CancellationToken cancellationToken)
        {
            List<Paper> papers;

            if (request.StartDate.HasValue && request.EndDate.HasValue)
            {
                papers = await _paperRepository.GetByDateRangeAsync(
                    request.StartDate.Value,
                    request.EndDate.Value);
            }
            else
            {
                papers = await _paperRepository.GetAllAsync();
            }

            return papers.Select(e => new PaperDto
            {
                Id = e.Id.Value,
                Date = e.Date,
                Usage = e.Usage
            }).ToList();
        }
    }
}