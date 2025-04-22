using System;
using System.Threading;
using System.Threading.Tasks;
using CarbonWise.BuildingBlocks.Domain.Papers;
using CarbonWise.BuildingBlocks.Infrastructure;
using MediatR;

namespace CarbonWise.BuildingBlocks.Application.Features.Papers.Commands.CreatePaper
{
    public class CreatePaperCommand : IRequest<PaperDto>
    {
        public DateTime Date { get; set; }
        public decimal Usage { get; set; }
    }

    public class CreatePaperCommandHandler : IRequestHandler<CreatePaperCommand, PaperDto>
    {
        private readonly IPaperRepository _paperRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreatePaperCommandHandler(
            IPaperRepository paperRepository,
            IUnitOfWork unitOfWork)
        {
            _paperRepository = paperRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<PaperDto> Handle(CreatePaperCommand request, CancellationToken cancellationToken)
        {
            var paper = Paper.Create(
                request.Date,
                request.Usage);

            await _paperRepository.AddAsync(paper);
            await _unitOfWork.CommitAsync(cancellationToken);

            return new PaperDto
            {
                Id = paper.Id.Value,
                Date = paper.Date,
                Usage = paper.Usage
            };
        }
    }
}