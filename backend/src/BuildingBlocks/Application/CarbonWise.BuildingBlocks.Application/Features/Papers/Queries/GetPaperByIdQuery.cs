using System;
using System.Threading;
using System.Threading.Tasks;
using CarbonWise.BuildingBlocks.Domain.Papers;
using MediatR;

namespace CarbonWise.BuildingBlocks.Application.Features.Papers.Queries.GetPaperById
{
    public class GetPaperByIdQuery : IRequest<PaperDto>
    {
        public Guid Id { get; set; }
    }

    public class GetPaperByIdQueryHandler : IRequestHandler<GetPaperByIdQuery, PaperDto>
    {
        private readonly IPaperRepository _paperRepository;

        public GetPaperByIdQueryHandler(IPaperRepository paperRepository)
        {
            _paperRepository = paperRepository;
        }

        public async Task<PaperDto> Handle(GetPaperByIdQuery request, CancellationToken cancellationToken)
        {
            var paper = await _paperRepository.GetByIdAsync(new PaperId(request.Id));
            if (paper == null)
            {
                return null;
            }

            return new PaperDto
            {
                Id = paper.Id.Value,
                Date = paper.Date,
                Usage = paper.Usage
            };
        }
    }
}