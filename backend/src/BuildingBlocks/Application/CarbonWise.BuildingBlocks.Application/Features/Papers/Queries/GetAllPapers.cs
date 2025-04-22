using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CarbonWise.BuildingBlocks.Domain.Papers;
using MediatR;

namespace CarbonWise.BuildingBlocks.Application.Features.Papers.Queries.GetAllPapers
{
    public class GetAllPapersQuery : IRequest<List<PaperDto>>
    {
    }

    public class GetAllPapersQueryHandler : IRequestHandler<GetAllPapersQuery, List<PaperDto>>
    {
        private readonly IPaperRepository _paperRepository;

        public GetAllPapersQueryHandler(IPaperRepository paperRepository)
        {
            _paperRepository = paperRepository;
        }

        public async Task<List<PaperDto>> Handle(GetAllPapersQuery request, CancellationToken cancellationToken)
        {
            var papers = await _paperRepository.GetAllAsync();

            return papers.Select(e => new PaperDto
            {
                Id = e.Id.Value,
                Date = e.Date,
                Usage = e.Usage
            }).ToList();
        }
    }
}