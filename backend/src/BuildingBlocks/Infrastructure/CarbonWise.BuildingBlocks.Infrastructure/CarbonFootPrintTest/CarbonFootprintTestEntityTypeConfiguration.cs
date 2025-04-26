using CarbonWise.BuildingBlocks.Domain.CarbonFootPrintTest;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CarbonWise.BuildingBlocks.Domain.Users;
using System.Text.Json;

namespace CarbonWise.BuildingBlocks.Infrastructure.CarbonFootPrintTest
{
    public class CarbonFootprintTestEntityTypeConfiguration : IEntityTypeConfiguration<CarbonFootprintTest>
    {
        public void Configure(EntityTypeBuilder<CarbonFootprintTest> builder)
        {
            builder.ToTable("CarbonFootprintTests");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasConversion(
                    id => id.Value,
                    dbId => new CarbonFootprintTestId(dbId));

            builder.Property(t => t.UserId)
                .HasConversion(
                    id => id.Value,
                    dbId => new UserId(dbId));

            builder.Property(t => t.CompletedAt)
                .IsRequired();

            builder.Property(t => t.TotalFootprint)
                .HasPrecision(18, 2);

            builder.HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(t => t.CategoryResults)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<Dictionary<string, decimal>>(v, (JsonSerializerOptions)null));
        }
    }

    public class TestResponseEntityTypeConfiguration : IEntityTypeConfiguration<TestResponse>
    {
        public void Configure(EntityTypeBuilder<TestResponse> builder)
        {
            builder.ToTable("TestResponses");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.Id)
                .HasConversion(
                    id => id.Value,
                    dbId => new TestResponseId(dbId));

            builder.Property(r => r.TestId)
                .HasConversion(
                    id => id.Value,
                    dbId => new CarbonFootprintTestId(dbId));

            builder.Property(r => r.QuestionId)
                .HasConversion(
                    id => id.Value,
                    dbId => new TestQuestionId(dbId));

            builder.Property(r => r.SelectedOptionId)
                .HasConversion(
                    id => id.Value,
                    dbId => new TestQuestionOptionId(dbId));

            builder.HasOne(r => r.Question)
                .WithMany()
                .HasForeignKey(r => r.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.SelectedOption)
                .WithMany()
                .HasForeignKey(r => r.SelectedOptionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class TestQuestionEntityTypeConfiguration : IEntityTypeConfiguration<TestQuestion>
    {
        public void Configure(EntityTypeBuilder<TestQuestion> builder)
        {
            builder.ToTable("TestQuestions");

            builder.HasKey(q => q.Id);

            builder.Property(q => q.Id)
                .HasConversion(
                    id => id.Value,
                    dbId => new TestQuestionId(dbId));

            builder.Property(q => q.Text)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(q => q.Category)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(q => q.DisplayOrder)
                .IsRequired();

            builder.HasMany(q => q.Options)
                .WithOne()
                .HasForeignKey(o => o.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class TestQuestionOptionEntityTypeConfiguration : IEntityTypeConfiguration<TestQuestionOption>
    {
        public void Configure(EntityTypeBuilder<TestQuestionOption> builder)
        {
            builder.ToTable("TestQuestionOptions");

            builder.HasKey(o => o.Id);

            builder.Property(o => o.Id)
                .HasConversion(
                    id => id.Value,
                    dbId => new TestQuestionOptionId(dbId));

            builder.Property(o => o.QuestionId)
                .HasConversion(
                    id => id.Value,
                    dbId => new TestQuestionId(dbId));

            builder.Property(o => o.Text)
                .IsRequired()
                .HasMaxLength(600);

            builder.Property(o => o.FootprintFactor)
                .IsRequired()
                .HasPrecision(18, 2);
        }
    }
}
