using CarbonWise.BuildingBlocks.Domain.Electrics;
using CarbonWise.BuildingBlocks.Infrastructure;
using MediatR;

namespace CarbonWise.BuildingBlocks.Application.Features.Electrics.Commands.DeleteElectric
{
    public class DeleteElectricCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }
    
        public class DeleteElectricCommandHandler : IRequestHandler<DeleteElectricCommand, bool>
        {
            private readonly IElectricRepository _electricRepository;
            private readonly IUnitOfWork _unitOfWork;

            public DeleteElectricCommandHandler(
                IElectricRepository electricRepository,
                IUnitOfWork unitOfWork)
            {
                _electricRepository = electricRepository;
                _unitOfWork = unitOfWork;
            }

            public async Task<bool> Handle(DeleteElectricCommand request, CancellationToken cancellationToken)
            {
                var electric = await _electricRepository.GetByIdAsync(new ElectricId(request.Id));
                if (electric == null)
                {
                    return false;
                }

                await _electricRepository.DeleteAsync(new ElectricId(request.Id));
                await _unitOfWork.CommitAsync(cancellationToken);

                return true;
            }
        }
}
