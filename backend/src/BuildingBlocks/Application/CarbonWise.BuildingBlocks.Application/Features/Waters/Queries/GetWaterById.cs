using System;
using System.Threading;
using System.Threading.Tasks;
using CarbonWise.BuildingBlocks.Domain.Waters;
using MediatR;

namespace CarbonWise.BuildingBlocks.Application.Features.Waters.Queries.GetWaterById
{
    public class GetWaterByIdQuery : IRequest<WaterDto>
    {
        public Guid Id { get; set; }
    }

    public class GetWaterByIdQueryHandler : IRequestHandler<GetWaterByIdQuery, WaterDto>
    {
        private readonly IWaterRepository _waterRepository;

        public GetWaterByIdQueryHandler(IWaterRepository waterRepository)
        {
            _waterRepository = waterRepository;
        }

        public async Task<WaterDto> Handle(GetWaterByIdQuery request, CancellationToken cancellationToken)
        {
            var water = await _waterRepository.GetByIdAsync(new WaterId(request.Id));
            if (water == null)
            {
                return null;
            }

            return new WaterDto
            {
                Id = water.Id.Value,
                Date = water.Date,
                InitialMeterValue = water.InitialMeterValue,
                FinalMeterValue = water.FinalMeterValue,
                Usage = water.Usage
            };
        }
    }
}