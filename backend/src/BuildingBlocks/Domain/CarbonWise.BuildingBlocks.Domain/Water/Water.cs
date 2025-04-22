namespace CarbonWise.BuildingBlocks.Domain.Waters
{
    public class Water : Entity, IAggregateRoot
    {
        public WaterId Id { get; private set; }
        public DateTime Date { get; private set; }
        public decimal InitialMeterValue { get; private set; }
        public decimal FinalMeterValue { get; private set; }
        public decimal Usage { get; private set; }

        protected Water() { }

        private Water(
            WaterId id,
            DateTime date,
            decimal initialMeterValue,
            decimal finalMeterValue,
            decimal usage)
        {
            Id = id;
            Date = date;
            InitialMeterValue = initialMeterValue;
            FinalMeterValue = finalMeterValue;
            Usage = usage;
        }

        public static Water Create(
            DateTime date,
            decimal initialMeterValue,
            decimal finalMeterValue)
        {
            if (initialMeterValue < 0)
                throw new ArgumentException("Initial meter value cannot be negative", nameof(initialMeterValue));

            if (finalMeterValue < initialMeterValue)
                throw new ArgumentException("Final meter value cannot be less than initial meter value", nameof(finalMeterValue));

            decimal usage = finalMeterValue - initialMeterValue;

            var water = new Water(
                new WaterId(Guid.NewGuid()),
                date,
                initialMeterValue,
                finalMeterValue,
                usage);

            water.AddDomainEvent(new WaterCreatedDomainEvent(water.Id));

            return water;
        }

        public void Update(
            DateTime date,
            decimal initialMeterValue,
            decimal finalMeterValue)
        {
            if (initialMeterValue < 0)
                throw new ArgumentException("Initial meter value cannot be negative", nameof(initialMeterValue));

            if (finalMeterValue < initialMeterValue)
                throw new ArgumentException("Final meter value cannot be less than initial meter value", nameof(finalMeterValue));

            Date = date;
            InitialMeterValue = initialMeterValue;
            FinalMeterValue = finalMeterValue;

            Usage = finalMeterValue - initialMeterValue;
        }
    }
}