using CarbonWise.BuildingBlocks.Domain.Buildings;
using CarbonWise.BuildingBlocks.Infrastructure;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonWise.BuildingBlocks.Application.Features.Buildings.Commands
{
    public class DeleteBuildingCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class DeleteBuildingCommandHandler : IRequestHandler<DeleteBuildingCommand, bool>
    {
        private readonly IBuildingRepository _buildingRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteBuildingCommandHandler(
            IBuildingRepository buildingRepository,
            IUnitOfWork unitOfWork)
        {
            _buildingRepository = buildingRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(DeleteBuildingCommand request, CancellationToken cancellationToken)
        {
            var building = await _buildingRepository.GetByIdAsync(new BuildingId(request.Id));
            if (building == null)
            {
                return false;
            }

            await _buildingRepository.DeleteAsync(new BuildingId(request.Id));
            await _unitOfWork.CommitAsync(cancellationToken);

            return true;
        }
    }
}
