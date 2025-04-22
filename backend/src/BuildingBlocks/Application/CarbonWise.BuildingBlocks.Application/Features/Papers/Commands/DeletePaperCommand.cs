using System;
using System.Threading;
using System.Threading.Tasks;
using CarbonWise.BuildingBlocks.Domain.Papers;
using CarbonWise.BuildingBlocks.Infrastructure;
using MediatR;

namespace CarbonWise.BuildingBlocks.Application.Features.Papers.Commands.DeletePaper
{
    public class DeletePaperCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class DeletePaperCommandHandler : IRequestHandler<DeletePaperCommand, bool>
    {
        private readonly IPaperRepository _paperRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeletePaperCommandHandler(
            IPaperRepository paperRepository,
            IUnitOfWork unitOfWork)
        {
            _paperRepository = paperRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(DeletePaperCommand request, CancellationToken cancellationToken)
        {
            var paper = await _paperRepository.GetByIdAsync(new PaperId(request.Id));
            if (paper == null)
            {
                return false;
            }

            await _paperRepository.DeleteAsync(new PaperId(request.Id));
            await _unitOfWork.CommitAsync(cancellationToken);

            return true;
        }
    }
}