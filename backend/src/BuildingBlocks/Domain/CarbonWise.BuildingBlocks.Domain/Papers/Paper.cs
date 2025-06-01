using System;
using CarbonWise.BuildingBlocks.Domain.Buildings;
using CarbonWise.BuildingBlocks.Domain;

namespace CarbonWise.BuildingBlocks.Domain.Papers
{
    public class Paper : Entity, IAggregateRoot
    {
        public PaperId Id { get; private set; }
        public DateTime Date { get; private set; }
        public decimal Usage { get; private set; }
        public BuildingId BuildingId { get; private set; }
        public virtual Building Building { get; private set; }

        protected Paper() { }

        private Paper(
            PaperId id,
            DateTime date,
            decimal usage,
            BuildingId buildingId)
        {
            Id = id;
            Date = date;
            Usage = usage;
            BuildingId = buildingId;
        }

        public static Paper Create(
            DateTime date,
            decimal usage,
            BuildingId buildingId)
        {
            if (usage < 0)
                throw new ArgumentException("Usage cannot be negative", nameof(usage));

            var paper = new Paper(
                new PaperId(Guid.NewGuid()),
                date,
                usage,
                buildingId);

            paper.AddDomainEvent(new PaperCreatedDomainEvent(paper.Id));

            return paper;
        }

        public void Update(
            DateTime date,
            decimal usage)
        {
            if (usage < 0)
                throw new ArgumentException("Usage cannot be negative", nameof(usage));

            Date = date;
            Usage = usage;
        }
    }
}