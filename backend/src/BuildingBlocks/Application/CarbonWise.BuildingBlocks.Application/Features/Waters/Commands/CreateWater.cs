using System;
using System.Threading;
using System.Threading.Tasks;
using CarbonWise.BuildingBlocks.Domain.Waters;
using CarbonWise.BuildingBlocks.Infrastructure;
using MediatR;

namespace CarbonWise.BuildingBlocks.Application.Features.Waters.Commands.CreateWater
{
    public class CreateWaterCommand : IRequest<WaterDto>
    {
        public DateTime Date { get; set; }
        public decimal InitialMeterValue { get; set; }
        public decimal FinalMeterValue { get; set; }
    }

    public class CreateWaterCommandHandler : IRequestHandler<CreateWaterCommand, WaterDto>
    {
        private readonly IWaterRepository _waterRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateWaterCommandHandler(
            IWaterRepository waterRepository,
            IUnitOfWork unitOfWork)
        {
            _waterRepository = waterRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<WaterDto> Handle(CreateWaterCommand request, CancellationToken cancellationToken)
        {
            var water = Water.Create(
                request.Date,
                request.InitialMeterValue,
                request.FinalMeterValue);

            await _waterRepository.AddAsync(water);
            await _unitOfWork.CommitAsync(cancellationToken);

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