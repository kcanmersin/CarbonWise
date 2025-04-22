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
    public class DeleteNaturalGasCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class DeleteNaturalGasCommandHandler : IRequestHandler<DeleteNaturalGasCommand, bool>
    {
        private readonly INaturalGasRepository _naturalGasRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteNaturalGasCommandHandler(
            INaturalGasRepository naturalGasRepository,
            IUnitOfWork unitOfWork)
        {
            _naturalGasRepository = naturalGasRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(DeleteNaturalGasCommand request, CancellationToken cancellationToken)
        {
            var naturalGas = await _naturalGasRepository.GetByIdAsync(new NaturalGasId(request.Id));
            if (naturalGas == null)
            {
                return false;
            }

            await _naturalGasRepository.DeleteAsync(new NaturalGasId(request.Id));
            await _unitOfWork.CommitAsync(cancellationToken);

            return true;
        }
    }
}
