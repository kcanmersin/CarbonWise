using CarbonWise.BuildingBlocks.Domain.CarbonFootPrintTest;
using CarbonWise.BuildingBlocks.Domain.Users;
using CarbonWise.BuildingBlocks.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonWise.BuildingBlocks.Application.Services.CarbonFootPrintTest
{
    public interface ICarbonFootprintTestService
    {
        Task<CarbonFootprintTestDto> StartNewTestAsync(Guid userId);
        Task<CarbonFootprintTestDto> SaveResponseAsync(Guid testId, Guid questionId, Guid optionId);
        Task<CarbonFootprintResultDto> CompleteTestAsync(Guid testId);
        Task<List<TestQuestionDto>> GetAllQuestionsAsync();
        Task<SustainabilityStatsDto> GetSustainabilityStatsAsync();
    }

    public class CarbonFootprintTestService : ICarbonFootprintTestService
    {
        private readonly ICarbonFootprintTestRepository _testRepository;
        private readonly ITestQuestionRepository _questionRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CarbonFootprintTestService(
            ICarbonFootprintTestRepository testRepository,
            ITestQuestionRepository questionRepository,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork)
        {
            _testRepository = testRepository;
            _questionRepository = questionRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<CarbonFootprintTestDto> StartNewTestAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(new UserId(userId));
            if (user == null)
            {
                throw new ApplicationException("User not found");
            }

            var test = CarbonFootprintTest.Create(new UserId(userId));

            await _testRepository.AddAsync(test);
            await _unitOfWork.CommitAsync();

            return new CarbonFootprintTestDto
            {
                Id = test.Id.Value,
                UserId = test.UserId.Value
            };
        }

        public async Task<CarbonFootprintTestDto> SaveResponseAsync(Guid testId, Guid questionId, Guid optionId)
        {
            var test = await _testRepository.GetByIdAsync(new CarbonFootprintTestId(testId));
            if (test == null)
            {
                throw new ApplicationException("Test not found");
            }

            var existingResponse = test.Responses.FirstOrDefault(r => r.QuestionId.Value == questionId);
            if (existingResponse != null)
            {
                await _testRepository.UpdateResponseAsync(existingResponse.Id, new TestQuestionOptionId(optionId));
            }
            else
            {
                test.AddResponse(new TestQuestionId(questionId), new TestQuestionOptionId(optionId));
            }

            await _unitOfWork.CommitAsync();

            return new CarbonFootprintTestDto
            {
                Id = test.Id.Value,
                UserId = test.UserId.Value,
                Responses = test.Responses.Select(r => new TestResponseDto
                {
                    QuestionId = r.QuestionId.Value,
                    SelectedOptionId = r.SelectedOptionId.Value
                }).ToList()
            };
        }

        public async Task<CarbonFootprintResultDto> CompleteTestAsync(Guid testId)
        {
            var test = await _testRepository.GetByIdWithDetailsAsync(new CarbonFootprintTestId(testId));
            if (test == null)
            {
                throw new ApplicationException("Test not found");
            }

            test.CalculateFootprint();

            var user = await _userRepository.GetByIdAsync(test.UserId);
            if (user == null)
            {
                throw new ApplicationException("User not found");
            }

            int sustainabilityPoints = CalculateSustainabilityPoints(test.TotalFootprint);
            user.UpdateSustainabilityPoint(sustainabilityPoints);
            await _userRepository.UpdateAsync(user);
            await _unitOfWork.CommitAsync();

            return new CarbonFootprintResultDto
            {
                Id = test.Id.Value,
                UserId = test.UserId.Value,
                TotalFootprint = test.TotalFootprint,
                SustainabilityPoints = sustainabilityPoints,
                CompletedAt = test.CompletedAt,
                CategoryResults = test.CategoryResults.Select(cr => new CategoryResultDto
                {
                    Category = cr.Key,
                    FootprintValue = cr.Value
                }).ToList()
            };
        }

        public async Task<List<TestQuestionDto>> GetAllQuestionsAsync()
        {
            var questions = await _questionRepository.GetAllOrderedAsync();

            return questions.Select(q => new TestQuestionDto
            {
                Id = q.Id.Value,
                Text = q.Text,
                Category = q.Category,
                DisplayOrder = q.DisplayOrder,
                Options = q.Options.Select(o => new TestQuestionOptionDto
                {
                    Id = o.Id.Value,
                    Text = o.Text
                }).ToList()
            }).ToList();
        }

        public async Task<SustainabilityStatsDto> GetSustainabilityStatsAsync()
        {
            var allUsers = await _userRepository.GetAllAsync();

            var usersWithSustainabilityPoints = allUsers.Where(u => u.SustainabilityPoint.HasValue).ToList();
            var totalSustainabilityPoints = usersWithSustainabilityPoints.Sum(u => u.SustainabilityPoint.Value);

            return new SustainabilityStatsDto
            {
                TotalUsers = allUsers.Count(),
                UsersWithSustainabilityPoints = usersWithSustainabilityPoints.Count(),
                UsersWithoutSustainabilityPoints = allUsers.Count() - usersWithSustainabilityPoints.Count(),
                TotalSustainabilityPoints = totalSustainabilityPoints,
                AverageSustainabilityPoints = usersWithSustainabilityPoints.Count() > 0
                    ? (double)totalSustainabilityPoints / usersWithSustainabilityPoints.Count()
                    : 0,
                HighestSustainabilityPoints = usersWithSustainabilityPoints.Count() > 0
                    ? usersWithSustainabilityPoints.Max(u => u.SustainabilityPoint.Value)
                    : 0,
                LowestSustainabilityPoints = usersWithSustainabilityPoints.Count() > 0
                    ? usersWithSustainabilityPoints.Min(u => u.SustainabilityPoint.Value)
                    : 0
            };
        }

        private int CalculateSustainabilityPoints(decimal totalFootprint)
        {
            if (totalFootprint <= 10)
                return 100;
            else if (totalFootprint <= 25)
                return 80;
            else if (totalFootprint <= 50)
                return 60;
            else if (totalFootprint <= 75)
                return 40;
            else
                return 20;
        }
    }

    public class CarbonFootprintTestDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public List<TestResponseDto> Responses { get; set; } = new List<TestResponseDto>();
    }

    public class TestResponseDto
    {
        public Guid QuestionId { get; set; }
        public Guid SelectedOptionId { get; set; }
    }

    public class CarbonFootprintResultDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public decimal TotalFootprint { get; set; }
        public int SustainabilityPoints { get; set; }
        public DateTime CompletedAt { get; set; }
        public List<CategoryResultDto> CategoryResults { get; set; } = new List<CategoryResultDto>();
    }

    public class CategoryResultDto
    {
        public string Category { get; set; }
        public decimal FootprintValue { get; set; }
    }

    public class TestQuestionDto
    {
        public Guid Id { get; set; }
        public string Text { get; set; }
        public string Category { get; set; }
        public int DisplayOrder { get; set; }
        public List<TestQuestionOptionDto> Options { get; set; } = new List<TestQuestionOptionDto>();
    }

    public class TestQuestionOptionDto
    {
        public Guid Id { get; set; }
        public string Text { get; set; }
    }

    public class SustainabilityStatsDto
    {
        public int TotalUsers { get; set; }
        public int UsersWithSustainabilityPoints { get; set; }
        public int UsersWithoutSustainabilityPoints { get; set; }
        public long TotalSustainabilityPoints { get; set; }
        public double AverageSustainabilityPoints { get; set; }
        public int HighestSustainabilityPoints { get; set; }
        public int LowestSustainabilityPoints { get; set; }
    }
}
