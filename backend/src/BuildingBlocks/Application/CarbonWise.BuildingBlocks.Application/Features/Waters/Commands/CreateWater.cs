using System;
using System.Threading;
using System.Threading.Tasks;
using CarbonWise.BuildingBlocks.Domain.Waters;
using CarbonWise.BuildingBlocks.Domain.Buildings;
using CarbonWise.BuildingBlocks.Infrastructure;
using MediatR;

namespace CarbonWise.BuildingBlocks.Application.Features.Waters.Commands.CreateWater
{
    public class CreateWaterCommand : IRequest<WaterDto>
    {
        public DateTime Date { get; set; }
        public decimal InitialMeterValue { get; set; }
        public decimal FinalMeterValue { get; set; }
        public Guid BuildingId { get; set; }
    }

    public class CreateWaterCommandHandler : IRequestHandler<CreateWaterCommand, WaterDto>
    {
        private readonly IWaterRepository _waterRepository;
        private readonly IBuildingRepository _buildingRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateWaterCommandHandler(
            IWaterRepository waterRepository,
            IBuildingRepository buildingRepository,
            IUnitOfWork unitOfWork)
        {
            _waterRepository = waterRepository;
            _buildingRepository = buildingRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<WaterDto> Handle(CreateWaterCommand request, CancellationToken cancellationToken)
        {
            var building = await _buildingRepository.GetByIdAsync(new BuildingId(request.BuildingId));
            if (building == null)
            {
                throw new ApplicationException("Building not found");
            }

            var buildingId = new BuildingId(request.BuildingId);
            var existsForMonth = await _waterRepository.ExistsForMonthAsync(buildingId, request.Date.Year, request.Date.Month);
            if (existsForMonth)
            {
                throw new ApplicationException($"Bu bina için {request.Date:yyyy/MM} tarihinde su verisi zaten mevcut. Aynı ay için birden fazla veri girilemez.");
            }

            var water = Water.Create(
                request.Date,
                request.InitialMeterValue,
                request.FinalMeterValue,
                new BuildingId(request.BuildingId));

            await _waterRepository.AddAsync(water);
            await _unitOfWork.CommitAsync(cancellationToken);

            return new WaterDto
            {
                Id = water.Id.Value,
                Date = water.Date,
                InitialMeterValue = water.InitialMeterValue,
                FinalMeterValue = water.FinalMeterValue,
                Usage = water.Usage,
                BuildingId = water.BuildingId.Value,
                BuildingName = building.Name
            };
        }
    }
}