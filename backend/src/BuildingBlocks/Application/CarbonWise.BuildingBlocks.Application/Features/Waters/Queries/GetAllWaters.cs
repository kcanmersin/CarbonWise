using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CarbonWise.BuildingBlocks.Domain.Waters;
using MediatR;

namespace CarbonWise.BuildingBlocks.Application.Features.Waters.Queries.GetAllWaters
{
    public class GetAllWatersQuery : IRequest<List<WaterDto>>
    {
    }

    public class GetAllWatersQueryHandler : IRequestHandler<GetAllWatersQuery, List<WaterDto>>
    {
        private readonly IWaterRepository _waterRepository;

        public GetAllWatersQueryHandler(IWaterRepository waterRepository)
        {
            _waterRepository = waterRepository;
        }

        public async Task<List<WaterDto>> Handle(GetAllWatersQuery request, CancellationToken cancellationToken)
        {
            var waters = await _waterRepository.GetAllAsync();

            return waters.Select(e => new WaterDto
            {
                Id = e.Id.Value,
                Date = e.Date,
                InitialMeterValue = e.InitialMeterValue,
                FinalMeterValue = e.FinalMeterValue,
                Usage = e.Usage
            }).ToList();
        }
    }
}
