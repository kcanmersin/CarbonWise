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
    public class FilterNaturalGasQuery : IRequest<List<NaturalGasDto>>
    {
        public Guid? BuildingId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class FilterNaturalGasQueryHandler : IRequestHandler<FilterNaturalGasQuery, List<NaturalGasDto>>
    {
        private readonly INaturalGasRepository _naturalGasRepository;
        private readonly IBuildingRepository _buildingRepository;

        public FilterNaturalGasQueryHandler(
            INaturalGasRepository naturalGasRepository,
            IBuildingRepository buildingRepository)
        {
            _naturalGasRepository = naturalGasRepository;
            _buildingRepository = buildingRepository;
        }

        public async Task<List<NaturalGasDto>> Handle(FilterNaturalGasQuery request, CancellationToken cancellationToken)
        {
            if (request.BuildingId.HasValue)
            {
                var building = await _buildingRepository.GetByIdAsync(new BuildingId(request.BuildingId.Value));
                if (building == null)
                {
                    throw new ApplicationException($"Building with id {request.BuildingId} not found");
                }
            }

            List<NaturalGas> naturalGasList;

            if (request.BuildingId.HasValue && request.StartDate.HasValue && request.EndDate.HasValue)
            {
                naturalGasList = await _naturalGasRepository.GetByBuildingIdAndDateRangeAsync(
                    new BuildingId(request.BuildingId.Value),
                    request.StartDate.Value,
                    request.EndDate.Value);
            }
            else if (request.BuildingId.HasValue)
            {
                naturalGasList = await _naturalGasRepository.GetByBuildingIdAsync(
                    new BuildingId(request.BuildingId.Value));
            }
            else if (request.StartDate.HasValue && request.EndDate.HasValue)
            {
                naturalGasList = await _naturalGasRepository.GetByDateRangeAsync(
                    request.StartDate.Value,
                    request.EndDate.Value);
            }
            else
            {
                throw new ApplicationException("At least one filter parameter is required");
            }

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
