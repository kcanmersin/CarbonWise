using System;
using System.Threading;
using System.Threading.Tasks;
using CarbonWise.BuildingBlocks.Domain.Waters;
using CarbonWise.BuildingBlocks.Infrastructure;
using MediatR;

namespace CarbonWise.BuildingBlocks.Application.Features.Waters.Commands.DeleteWater
{
    public class DeleteWaterCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class DeleteWaterCommandHandler : IRequestHandler<DeleteWaterCommand, bool>
    {
        private readonly IWaterRepository _waterRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteWaterCommandHandler(
            IWaterRepository waterRepository,
            IUnitOfWork unitOfWork)
        {
            _waterRepository = waterRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(DeleteWaterCommand request, CancellationToken cancellationToken)
        {
            var water = await _waterRepository.GetByIdAsync(new WaterId(request.Id));
            if (water == null)
            {
                return false;
            }

            await _waterRepository.DeleteAsync(new WaterId(request.Id));
            await _unitOfWork.CommitAsync(cancellationToken);

            return true;
        }
    }
}
