using CarbonWise.BuildingBlocks.Domain.Papers;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonWise.BuildingBlocks.Application.Features.Papers.Queries
{
    public class GetPaperMonthlyTotalsQuery : IRequest<List<PaperMonthlyTotalDto>>
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class GetPaperMonthlyTotalsQueryHandler : IRequestHandler<GetPaperMonthlyTotalsQuery, List<PaperMonthlyTotalDto>>
    {
        private readonly IPaperRepository _paperRepository;

        public GetPaperMonthlyTotalsQueryHandler(IPaperRepository paperRepository)
        {
            _paperRepository = paperRepository;
        }

        public async Task<List<PaperMonthlyTotalDto>> Handle(GetPaperMonthlyTotalsQuery request, CancellationToken cancellationToken)
        {
            return await _paperRepository.GetMonthlyTotalsAsync(request.StartDate, request.EndDate);
        }
    }
}
