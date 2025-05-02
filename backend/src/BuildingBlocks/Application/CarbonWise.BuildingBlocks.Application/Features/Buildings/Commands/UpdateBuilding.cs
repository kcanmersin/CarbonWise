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
    public class UpdateBuildingCommand : IRequest<BuildingDto>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string E_MeterCode { get; set; }
        public string G_MeterCode { get; set; }
    }

    public class UpdateBuildingCommandHandler : IRequestHandler<UpdateBuildingCommand, BuildingDto>
    {
        private readonly IBuildingRepository _buildingRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateBuildingCommandHandler(
            IBuildingRepository buildingRepository,
            IUnitOfWork unitOfWork)
        {
            _buildingRepository = buildingRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<BuildingDto> Handle(UpdateBuildingCommand request, CancellationToken cancellationToken)
        {
            var building = await _buildingRepository.GetByIdAsync(new BuildingId(request.Id));
            if (building == null)
            {
                throw new ApplicationException($"Building with ID {request.Id} not found");
            }

            try
            {
                building.Update(
                    request.Name,
                    request.E_MeterCode,
                    request.G_MeterCode);

                await _buildingRepository.UpdateAsync(building);
                await _unitOfWork.CommitAsync(cancellationToken);

                return new BuildingDto
                {
                    Id = building.Id.Value,
                    Name = building.Name,
                    E_MeterCode = building.E_MeterCode,
                    G_MeterCode = building.G_MeterCode
                };
            }
            catch (ArgumentException ex)
            {
                throw new ApplicationException(ex.Message);
            }
        }
    }
}
