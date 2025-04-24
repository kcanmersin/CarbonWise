using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CarbonWise.BuildingBlocks.Domain.Electrics;
using MediatR;

namespace CarbonWise.BuildingBlocks.Application.Features.Electrics.Queries
{
    public class GetElectricMonthlyAggregateQuery : IRequest<List<ElectricMonthlyAggregateDto>>
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class GetElectricMonthlyAggregateQueryHandler : IRequestHandler<GetElectricMonthlyAggregateQuery, List<ElectricMonthlyAggregateDto>>
    {
        private readonly IElectricRepository _electricRepository;

        public GetElectricMonthlyAggregateQueryHandler(IElectricRepository electricRepository)
        {
            _electricRepository = electricRepository;
        }

        public async Task<List<ElectricMonthlyAggregateDto>> Handle(GetElectricMonthlyAggregateQuery request, CancellationToken cancellationToken)
        {
            return await _electricRepository.GetMonthlyAggregateAsync(request.StartDate, request.EndDate);
        }
    }
}