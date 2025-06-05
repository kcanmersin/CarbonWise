using System;
using System.Threading;
using System.Threading.Tasks;
using CarbonWise.BuildingBlocks.Domain.Papers;
using CarbonWise.BuildingBlocks.Domain.Buildings;
using CarbonWise.BuildingBlocks.Infrastructure;
using MediatR;

namespace CarbonWise.BuildingBlocks.Application.Features.Papers.Commands.CreatePaper
{
    public class CreatePaperCommand : IRequest<PaperDto>
    {
        public DateTime Date { get; set; }
        public decimal Usage { get; set; }
        public Guid BuildingId { get; set; }
    }

    public class CreatePaperCommandHandler : IRequestHandler<CreatePaperCommand, PaperDto>
    {
        private readonly IPaperRepository _paperRepository;
        private readonly IBuildingRepository _buildingRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreatePaperCommandHandler(
            IPaperRepository paperRepository,
            IBuildingRepository buildingRepository,
            IUnitOfWork unitOfWork)
        {
            _paperRepository = paperRepository;
            _buildingRepository = buildingRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<PaperDto> Handle(CreatePaperCommand request, CancellationToken cancellationToken)
        {
            var building = await _buildingRepository.GetByIdAsync(new BuildingId(request.BuildingId));
            if (building == null)
            {
                throw new ApplicationException("Building not found");
            }

            var paper = Paper.Create(
                request.Date,
                request.Usage,
                new BuildingId(request.BuildingId));

            await _paperRepository.AddAsync(paper);
            await _unitOfWork.CommitAsync(cancellationToken);

            return new PaperDto
            {
                Id = paper.Id.Value,
                Date = paper.Date,
                Usage = paper.Usage,
                BuildingId = paper.BuildingId.Value,
                BuildingName = building.Name
            };
        }
    }
}