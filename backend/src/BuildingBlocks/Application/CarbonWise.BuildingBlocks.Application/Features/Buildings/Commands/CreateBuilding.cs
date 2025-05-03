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
    public class CreateBuildingCommand : IRequest<BuildingDto>
    {
        public string Name { get; set; }
        public string E_MeterCode { get; set; }
        public string G_MeterCode { get; set; }
    }

    public class CreateBuildingCommandHandler : IRequestHandler<CreateBuildingCommand, BuildingDto>
    {
        private readonly IBuildingRepository _buildingRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateBuildingCommandHandler(
            IBuildingRepository buildingRepository,
            IUnitOfWork unitOfWork)
        {
            _buildingRepository = buildingRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<BuildingDto> Handle(CreateBuildingCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var building = Building.Create(
                    request.Name,
                    request.E_MeterCode,
                    request.G_MeterCode);

                await _buildingRepository.AddAsync(building);
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
