using CarbonWise.BuildingBlocks.Domain.NaturalGases;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonWise.BuildingBlocks.Application.Features.NaturalGases.Queries
{
    public class GetNaturalGasByIdQuery : IRequest<NaturalGasDto>
    {
        public Guid Id { get; set; }
    }

    public class GetNaturalGasByIdQueryHandler : IRequestHandler<GetNaturalGasByIdQuery, NaturalGasDto>
    {
        private readonly INaturalGasRepository _naturalGasRepository;

        public GetNaturalGasByIdQueryHandler(INaturalGasRepository naturalGasRepository)
        {
            _naturalGasRepository = naturalGasRepository;
        }

        public async Task<NaturalGasDto> Handle(GetNaturalGasByIdQuery request, CancellationToken cancellationToken)
        {
            var naturalGas = await _naturalGasRepository.GetByIdAsync(new NaturalGasId(request.Id));
            if (naturalGas == null)
            {
                return null;
            }

            return new NaturalGasDto
            {
                Id = naturalGas.Id.Value,
                Date = naturalGas.Date,
                InitialMeterValue = naturalGas.InitialMeterValue,
                FinalMeterValue = naturalGas.FinalMeterValue,
                Usage = naturalGas.Usage,
                SM3Value = naturalGas.SM3Value,
                BuildingId = naturalGas.BuildingId.Value,
                BuildingName = naturalGas.Building?.Name
            };
        }
    }
}
