using CarbonWise.BuildingBlocks.Domain.Buildings;
using CarbonWise.BuildingBlocks.Domain.Waters;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonWise.BuildingBlocks.Application.Features.Waters.Queries
{
    public class GetWatersByBuildingQuery : IRequest<List<WaterDto>>
    {
        public Guid BuildingId { get; set; }
    }

    public class GetWatersByBuildingQueryHandler : IRequestHandler<GetWatersByBuildingQuery, List<WaterDto>>
    {
        private readonly IWaterRepository _waterRepository;
        private readonly IBuildingRepository _buildingRepository;

        public GetWatersByBuildingQueryHandler(
            IWaterRepository waterRepository,
            IBuildingRepository buildingRepository)
        {
            _waterRepository = waterRepository;
            _buildingRepository = buildingRepository;
        }

        public async Task<List<WaterDto>> Handle(GetWatersByBuildingQuery request, CancellationToken cancellationToken)
        {
            var building = await _buildingRepository.GetByIdAsync(new BuildingId(request.BuildingId));
            if (building == null)
            {
                throw new ApplicationException($"Building with id {request.BuildingId} not found");
            }

            var waters = await _waterRepository.GetByBuildingIdAsync(new BuildingId(request.BuildingId));

            return waters.Select(w => new WaterDto
            {
                Id = w.Id.Value,
                Date = w.Date,
                InitialMeterValue = w.InitialMeterValue,
                FinalMeterValue = w.FinalMeterValue,
                Usage = w.Usage,
                BuildingId = w.BuildingId.Value,
                BuildingName = w.Building?.Name
            }).ToList();
        }
    }
}
