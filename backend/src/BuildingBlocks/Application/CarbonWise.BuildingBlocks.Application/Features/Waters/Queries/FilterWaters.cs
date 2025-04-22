using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CarbonWise.BuildingBlocks.Domain.Waters;
using MediatR;

namespace CarbonWise.BuildingBlocks.Application.Features.Waters.Queries.FilterWaters
{
    public class FilterWatersQuery : IRequest<List<WaterDto>>
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class FilterWatersQueryHandler : IRequestHandler<FilterWatersQuery, List<WaterDto>>
    {
        private readonly IWaterRepository _waterRepository;

        public FilterWatersQueryHandler(IWaterRepository waterRepository)
        {
            _waterRepository = waterRepository;
        }

        public async Task<List<WaterDto>> Handle(FilterWatersQuery request, CancellationToken cancellationToken)
        {
            List<Water> waters;

            if (request.StartDate.HasValue && request.EndDate.HasValue)
            {
                waters = await _waterRepository.GetByDateRangeAsync(
                    request.StartDate.Value,
                    request.EndDate.Value);
            }
            else
            {
                waters = await _waterRepository.GetAllAsync();
            }

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