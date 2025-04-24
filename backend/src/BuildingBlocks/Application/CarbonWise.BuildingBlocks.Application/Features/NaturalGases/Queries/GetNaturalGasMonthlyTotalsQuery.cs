// GetNaturalGasMonthlyTotalsQuery.cs
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CarbonWise.BuildingBlocks.Domain.NaturalGases;
using MediatR;

namespace CarbonWise.BuildingBlocks.Application.Features.NaturalGases.Queries
{
    public class GetNaturalGasMonthlyTotalsQuery : IRequest<List<NaturalGasMonthlyTotalDto>>
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class GetNaturalGasMonthlyTotalsQueryHandler : IRequestHandler<GetNaturalGasMonthlyTotalsQuery, List<NaturalGasMonthlyTotalDto>>
    {
        private readonly INaturalGasRepository _naturalGasRepository;

        public GetNaturalGasMonthlyTotalsQueryHandler(INaturalGasRepository naturalGasRepository)
        {
            _naturalGasRepository = naturalGasRepository;
        }

        public async Task<List<NaturalGasMonthlyTotalDto>> Handle(GetNaturalGasMonthlyTotalsQuery request, CancellationToken cancellationToken)
        {
            return await _naturalGasRepository.GetMonthlyTotalsAsync(request.StartDate, request.EndDate);
        }
    }
}