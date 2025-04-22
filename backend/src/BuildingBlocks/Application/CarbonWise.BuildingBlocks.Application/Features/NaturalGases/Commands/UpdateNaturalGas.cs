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
    public class UpdateNaturalGasCommand : IRequest<NaturalGasDto>
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public decimal InitialMeterValue { get; set; }
        public decimal FinalMeterValue { get; set; }
        public decimal SM3Value { get; set; }
    }

    public class UpdateNaturalGasCommandHandler : IRequestHandler<UpdateNaturalGasCommand, NaturalGasDto>
    {
        private readonly INaturalGasRepository _naturalGasRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateNaturalGasCommandHandler(
            INaturalGasRepository naturalGasRepository,
            IUnitOfWork unitOfWork)
        {
            _naturalGasRepository = naturalGasRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<NaturalGasDto> Handle(UpdateNaturalGasCommand request, CancellationToken cancellationToken)
        {
            var naturalGas = await _naturalGasRepository.GetByIdAsync(new NaturalGasId(request.Id));
            if (naturalGas == null)
            {
                throw new ApplicationException($"NaturalGas with id {request.Id} not found");
            }

            naturalGas.Update(
                request.Date,
                request.InitialMeterValue,
                request.FinalMeterValue,
                request.SM3Value);

            await _naturalGasRepository.UpdateAsync(naturalGas);
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
                BuildingName = naturalGas.Building?.Name
            };
        }
    }
}
