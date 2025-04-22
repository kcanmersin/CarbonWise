using CarbonWise.BuildingBlocks.Domain.Electrics;
using CarbonWise.BuildingBlocks.Infrastructure;
using MediatR;

namespace CarbonWise.BuildingBlocks.Application.Features.Electrics.Commands.UpdateElectric
{
    public class UpdateElectricCommand : IRequest<ElectricDto>
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public decimal InitialMeterValue { get; set; }
        public decimal FinalMeterValue { get; set; }
        public decimal KWHValue { get; set; }
    }

        public class UpdateElectricCommandHandler : IRequestHandler<UpdateElectricCommand, ElectricDto>
        {
            private readonly IElectricRepository _electricRepository;
            private readonly IUnitOfWork _unitOfWork;

            public UpdateElectricCommandHandler(
                IElectricRepository electricRepository,
                IUnitOfWork unitOfWork)
            {
                _electricRepository = electricRepository;
                _unitOfWork = unitOfWork;
            }

            public async Task<ElectricDto> Handle(UpdateElectricCommand request, CancellationToken cancellationToken)
            {
                var electric = await _electricRepository.GetByIdAsync(new ElectricId(request.Id));
                if (electric == null)
                {
                    throw new ApplicationException($"Electric with id {request.Id} not found");
                }

                electric.Update(
                    request.Date,
                    request.InitialMeterValue,
                    request.FinalMeterValue,
                    request.KWHValue);

                await _electricRepository.UpdateAsync(electric);
                await _unitOfWork.CommitAsync(cancellationToken);

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
