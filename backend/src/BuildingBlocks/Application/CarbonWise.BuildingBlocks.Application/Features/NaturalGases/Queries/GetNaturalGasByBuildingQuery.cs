using CarbonWise.BuildingBlocks.Domain.Buildings;
using CarbonWise.BuildingBlocks.Domain.NaturalGases;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonWise.BuildingBlocks.Application.Features.NaturalGases.Queries
{
    public class GetNaturalGasByBuildingQuery : IRequest<List<NaturalGasDto>>
    {
        public Guid BuildingId { get; set; }
    }

    public class GetNaturalGasByBuildingQueryHandler : IRequestHandler<GetNaturalGasByBuildingQuery, List<NaturalGasDto>>
    {
        private readonly INaturalGasRepository _naturalGasRepository;
        private readonly IBuildingRepository _buildingRepository;

        public GetNaturalGasByBuildingQueryHandler(
            INaturalGasRepository naturalGasRepository,
            IBuildingRepository buildingRepository)
        {
            _naturalGasRepository = naturalGasRepository;
            _buildingRepository = buildingRepository;
        }

        public async Task<List<NaturalGasDto>> Handle(GetNaturalGasByBuildingQuery request, CancellationToken cancellationToken)
        {
            var building = await _buildingRepository.GetByIdAsync(new BuildingId(request.BuildingId));
            if (building == null)
            {
                throw new ApplicationException($"Building with id {request.BuildingId} not found");
            }

            var naturalGasList = await _naturalGasRepository.GetByBuildingIdAsync(new BuildingId(request.BuildingId));

            return naturalGasList.Select(e => new NaturalGasDto
            {
                Id = e.Id.Value,
                Date = e.Date,
                InitialMeterValue = e.InitialMeterValue,
                FinalMeterValue = e.FinalMeterValue,
                Usage = e.Usage,
                SM3Value = e.SM3Value,
                BuildingId = e.BuildingId.Value,
                BuildingName = e.Building?.Name
            }).ToList();
        }
    }
}
