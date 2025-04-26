using CarbonWise.BuildingBlocks.Domain.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonWise.BuildingBlocks.Domain.CarbonFootPrintTest
{
    public class CarbonFootprintTest : Entity, IAggregateRoot
    {
        public CarbonFootprintTestId Id { get; private set; }
        public UserId UserId { get; private set; }
        public virtual User User { get; private set; }
        public DateTime CompletedAt { get; private set; }
        public decimal TotalFootprint { get; private set; } // in kg CO2e
        public Dictionary<string, decimal> CategoryResults { get; private set; }
        public virtual ICollection<TestResponse> Responses { get; private set; }

        protected CarbonFootprintTest() { }

        private CarbonFootprintTest(
            CarbonFootprintTestId id,
            UserId userId)
        {
            Id = id;
            UserId = userId;
            CompletedAt = DateTime.UtcNow;
            CategoryResults = new Dictionary<string, decimal>();
            Responses = new List<TestResponse>();
        }

        public static CarbonFootprintTest Create(UserId userId)
        {
            return new CarbonFootprintTest(
                new CarbonFootprintTestId(Guid.NewGuid()),
                userId);
        }

        public void AddResponse(TestQuestionId questionId, TestQuestionOptionId optionId)
        {
            var response = TestResponse.Create(Id, questionId, optionId);
            Responses.Add(response);
        }

        public void CalculateFootprint()
        {
            decimal total = 0;
            CategoryResults.Clear();

            var categories = Responses.GroupBy(r => r.Question.Category);

            foreach (var category in categories)
            {
                decimal categoryTotal = category.Sum(r => r.SelectedOption.FootprintFactor);
                total += categoryTotal;
                CategoryResults[category.Key] = categoryTotal;
            }

            TotalFootprint = total;
            CompletedAt = DateTime.UtcNow;
        }
    }

    public class TestResponse : Entity
    {
        public TestResponseId Id { get; private set; }
        public CarbonFootprintTestId TestId { get; private set; }
        public TestQuestionId QuestionId { get; private set; }
        public TestQuestionOptionId SelectedOptionId { get; private set; }

        public virtual TestQuestion Question { get; private set; }
        public virtual TestQuestionOption SelectedOption { get; private set; }

        protected TestResponse() { }

        private TestResponse(
            TestResponseId id,
            CarbonFootprintTestId testId,
            TestQuestionId questionId,
            TestQuestionOptionId selectedOptionId)
        {
            Id = id;
            TestId = testId;
            QuestionId = questionId;
            SelectedOptionId = selectedOptionId;
        }

        public static TestResponse Create(
            CarbonFootprintTestId testId,
            TestQuestionId questionId,
            TestQuestionOptionId selectedOptionId)
        {
            return new TestResponse(
                new TestResponseId(Guid.NewGuid()),
                testId,
                questionId,
                selectedOptionId);
        }
    }

    public class CarbonFootprintTestId : TypedIdValueBase
    {
        public CarbonFootprintTestId(Guid value) : base(value) { }
    }

    public class TestResponseId : TypedIdValueBase
    {
        public TestResponseId(Guid value) : base(value) { }
    }

    public class TestQuestion : Entity
    {
        public TestQuestionId Id { get; private set; }
        public string Text { get; private set; }
        public string Category { get; private set; }
        public int DisplayOrder { get; private set; }
        public virtual ICollection<TestQuestionOption> Options { get; private set; }
    }

    public class TestQuestionOption : Entity
    {
        public TestQuestionOptionId Id { get; private set; }
        public TestQuestionId QuestionId { get; private set; }
        public string Text { get; private set; }
        public decimal FootprintFactor { get; private set; }
    }

    public class TestQuestionId : TypedIdValueBase
    {
        public TestQuestionId(Guid value) : base(value) { }
    }

    public class TestQuestionOptionId : TypedIdValueBase
    {
        public TestQuestionOptionId(Guid value) : base(value) { }
    }
}
