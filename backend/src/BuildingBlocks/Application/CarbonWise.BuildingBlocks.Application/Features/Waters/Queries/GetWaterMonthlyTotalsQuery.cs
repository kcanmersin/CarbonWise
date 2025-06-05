using CarbonWise.BuildingBlocks.Domain.Waters;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonWise.BuildingBlocks.Application.Features.Waters.Queries
{
    public class GetWaterMonthlyTotalsQuery : IRequest<List<WaterMonthlyTotalDto>>
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class GetWaterMonthlyTotalsQueryHandler : IRequestHandler<GetWaterMonthlyTotalsQuery, List<WaterMonthlyTotalDto>>
    {
        private readonly IWaterRepository _waterRepository;

        public GetWaterMonthlyTotalsQueryHandler(IWaterRepository waterRepository)
        {
            _waterRepository = waterRepository;
        }

        public async Task<List<WaterMonthlyTotalDto>> Handle(GetWaterMonthlyTotalsQuery request, CancellationToken cancellationToken)
        {
            return await _waterRepository.GetMonthlyTotalsAsync(request.StartDate, request.EndDate);
        }
    }
}
