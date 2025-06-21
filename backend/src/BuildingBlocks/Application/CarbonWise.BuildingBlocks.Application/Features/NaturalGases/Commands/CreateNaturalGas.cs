using CarbonWise.BuildingBlocks.Domain.Buildings;
using CarbonWise.BuildingBlocks.Domain.NaturalGases;
using CarbonWise.BuildingBlocks.Infrastructure;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonWise.BuildingBlocks.Application.Features.NaturalGases.Commands
{
    public class CreateNaturalGasCommand : IRequest<NaturalGasDto>
    {
        public DateTime Date { get; set; }
        public decimal InitialMeterValue { get; set; }
        public decimal FinalMeterValue { get; set; }
        public decimal SM3Value { get; set; }
        public Guid BuildingId { get; set; }
    }

    public class CreateNaturalGasCommandHandler : IRequestHandler<CreateNaturalGasCommand, NaturalGasDto>
    {
        private readonly INaturalGasRepository _naturalGasRepository;
        private readonly IBuildingRepository _buildingRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateNaturalGasCommandHandler(
            INaturalGasRepository naturalGasRepository,
            IBuildingRepository buildingRepository,
            IUnitOfWork unitOfWork)
        {
            _naturalGasRepository = naturalGasRepository;
            _buildingRepository = buildingRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<NaturalGasDto> Handle(CreateNaturalGasCommand request, CancellationToken cancellationToken)
        {
            var building = await _buildingRepository.GetByIdAsync(new BuildingId(request.BuildingId));
            if (building == null)
            {
                throw new ApplicationException("Building not found");
            }

            var buildingId = new BuildingId(request.BuildingId);
            var existsForMonth = await _naturalGasRepository.ExistsForMonthAsync(buildingId, request.Date.Year, request.Date.Month);
            if (existsForMonth)
            {
                throw new ApplicationException($"Bu bina için {request.Date:yyyy/MM} tarihinde doğalgaz verisi zaten mevcut. Aynı ay için birden fazla veri girilemez.");
            }

            var naturalGas = NaturalGas.Create(
                request.Date,
                request.InitialMeterValue,
                request.FinalMeterValue,
                request.SM3Value,
                new BuildingId(request.BuildingId));

            await _naturalGasRepository.AddAsync(naturalGas);
            await _unitOfWork.CommitAsync(cancellationToken);

            return new NaturalGasDto
            {
                Id = naturalGas.Id.Value,
                Date = naturalGas.Date,
                InitialMeterValue = naturalGas.InitialMeterValue,
                FinalMeterValue = naturalGas.FinalMeterValue,
                Usage = naturalGas.Usage,
                SM3Value = naturalGas.SM3Value,
                BuildingId = naturalGas.BuildingId.Value,
                BuildingName = building.Name
            };
        }
    }
}
