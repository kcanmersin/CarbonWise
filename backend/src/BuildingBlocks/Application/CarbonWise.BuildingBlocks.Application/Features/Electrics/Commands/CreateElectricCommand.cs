using System;
using MediatR;
using CarbonWise.BuildingBlocks.Domain.Electrics;
using CarbonWise.BuildingBlocks.Domain.Buildings;
using CarbonWise.BuildingBlocks.Infrastructure;

namespace CarbonWise.BuildingBlocks.Application.Features.Electrics.Commands.CreateElectric
{
    public class CreateElectricCommand : IRequest<ElectricDto>
    {
        public DateTime Date { get; set; }
        public decimal InitialMeterValue { get; set; }
        public decimal FinalMeterValue { get; set; }
        public decimal KWHValue { get; set; }
        public Guid BuildingId { get; set; }
    }

    public class CreateElectricCommandHandler : IRequestHandler<CreateElectricCommand, ElectricDto>
    {
        private readonly IElectricRepository _electricRepository;
        private readonly IBuildingRepository _buildingRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateElectricCommandHandler(
            IElectricRepository electricRepository,
            IBuildingRepository buildingRepository,
            IUnitOfWork unitOfWork)
        {
            _electricRepository = electricRepository;
            _buildingRepository = buildingRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ElectricDto> Handle(CreateElectricCommand request, CancellationToken cancellationToken)
        {
            var building = await _buildingRepository.GetByIdAsync(new BuildingId(request.BuildingId));
            if (building == null)
            {
                throw new ApplicationException("Building not found");
            }

            var electric = Electric.Create(
                request.Date,
                request.InitialMeterValue,
                request.FinalMeterValue,
                request.KWHValue,
                new BuildingId(request.BuildingId));

            await _electricRepository.AddAsync(electric);
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
                BuildingName = building.Name
            };
        }
    }
}


