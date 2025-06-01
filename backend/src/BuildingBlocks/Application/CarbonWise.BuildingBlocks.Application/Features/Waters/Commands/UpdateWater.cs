using System;
using System.Threading;
using System.Threading.Tasks;
using CarbonWise.BuildingBlocks.Domain.Waters;
using CarbonWise.BuildingBlocks.Infrastructure;
using MediatR;

namespace CarbonWise.BuildingBlocks.Application.Features.Waters.Commands.UpdateWater
{
    public class UpdateWaterCommand : IRequest<WaterDto>
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public decimal InitialMeterValue { get; set; }
        public decimal FinalMeterValue { get; set; }
    }

    public class UpdateWaterCommandHandler : IRequestHandler<UpdateWaterCommand, WaterDto>
    {
        private readonly IWaterRepository _waterRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateWaterCommandHandler(
            IWaterRepository waterRepository,
            IUnitOfWork unitOfWork)
        {
            _waterRepository = waterRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<WaterDto> Handle(UpdateWaterCommand request, CancellationToken cancellationToken)
        {
            var water = await _waterRepository.GetByIdAsync(new WaterId(request.Id));
            if (water == null)
            {
                throw new ApplicationException($"Water with id {request.Id} not found");
            }

            water.Update(
                request.Date,
                request.InitialMeterValue,
                request.FinalMeterValue);

            await _waterRepository.UpdateAsync(water);
            await _unitOfWork.CommitAsync(cancellationToken);

            return new WaterDto
            {
                Id = water.Id.Value,
                Date = water.Date,
                InitialMeterValue = water.InitialMeterValue,
                FinalMeterValue = water.FinalMeterValue,
                Usage = water.Usage,
                BuildingId = water.BuildingId.Value,
                BuildingName = water.Building?.Name
            };
        }
    }
}