using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CarbonWise.BuildingBlocks.Domain.Electrics;
using MediatR;

namespace CarbonWise.BuildingBlocks.Application.Features.Electrics.Queries
{
    public class GetElectricMonthlyTotalsQuery : IRequest<List<ElectricMonthlyTotalDto>>
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class GetElectricMonthlyTotalsQueryHandler : IRequestHandler<GetElectricMonthlyTotalsQuery, List<ElectricMonthlyTotalDto>>
    {
        private readonly IElectricRepository _electricRepository;

        public GetElectricMonthlyTotalsQueryHandler(IElectricRepository electricRepository)
        {
            _electricRepository = electricRepository;
        }

        public async Task<List<ElectricMonthlyTotalDto>> Handle(GetElectricMonthlyTotalsQuery request, CancellationToken cancellationToken)
        {
            return await _electricRepository.GetMonthlyTotalsAsync(request.StartDate, request.EndDate);
        }
    }
}