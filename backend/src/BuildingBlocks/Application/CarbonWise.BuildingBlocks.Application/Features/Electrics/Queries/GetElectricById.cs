using CarbonWise.BuildingBlocks.Domain.Electrics;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonWise.BuildingBlocks.Application.Features.Electrics.Queries
{
        public class GetElectricByIdQuery : IRequest<ElectricDto>
        {
            public Guid Id { get; set; }
        }



        public class GetElectricByIdQueryHandler : IRequestHandler<GetElectricByIdQuery, ElectricDto>
        {
            private readonly IElectricRepository _electricRepository;

            public GetElectricByIdQueryHandler(IElectricRepository electricRepository)
            {
                _electricRepository = electricRepository;
            }

            public async Task<ElectricDto> Handle(GetElectricByIdQuery request, CancellationToken cancellationToken)
            {
                var electric = await _electricRepository.GetByIdAsync(new ElectricId(request.Id));
                if (electric == null)
                {
                    return null;
                }

                return new ElectricDto
                {
                    Id = electric.Id.Value,
                    Date = electric.Date,
                    InitialMeterValue = electric.InitialMeterValue,
                    FinalMeterValue = electric.FinalMeterValue,
                    Usage = electric.Usage,
                    KWHValue = electric.KWHValue,
                    BuildingId = electric.BuildingId.Value,
                    BuildingName = electric.Building?.Name
                };
            }
        }
    }
