using System;
using System.Threading;
using System.Threading.Tasks;
using CarbonWise.BuildingBlocks.Domain.Papers;
using CarbonWise.BuildingBlocks.Infrastructure;
using MediatR;

namespace CarbonWise.BuildingBlocks.Application.Features.Papers.Commands.UpdatePaper
{
    public class UpdatePaperCommand : IRequest<PaperDto>
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public decimal Usage { get; set; }
    }

    public class UpdatePaperCommandHandler : IRequestHandler<UpdatePaperCommand, PaperDto>
    {
        private readonly IPaperRepository _paperRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdatePaperCommandHandler(
            IPaperRepository paperRepository,
            IUnitOfWork unitOfWork)
        {
            _paperRepository = paperRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<PaperDto> Handle(UpdatePaperCommand request, CancellationToken cancellationToken)
        {
            var paper = await _paperRepository.GetByIdAsync(new PaperId(request.Id));
            if (paper == null)
            {
                throw new ApplicationException($"Paper with id {request.Id} not found");
            }

            paper.Update(
                request.Date,
                request.Usage);

            await _paperRepository.UpdateAsync(paper);
            await _unitOfWork.CommitAsync(cancellationToken);

            return new PaperDto
            {
                Id = paper.Id.Value,
                Date = paper.Date,
                Usage = paper.Usage,
                BuildingId = paper.BuildingId.Value,
                BuildingName = paper.Building?.Name
            };
        }
    }
}