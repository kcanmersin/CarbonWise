namespace CarbonWise.BuildingBlocks.Application
{
    public class InvalidCommandException : Exception
    {
        public IDictionary<string, string[]> Errors { get; }

        public InvalidCommandException(IDictionary<string, string[]> errors)
        {
            Errors = errors;
        }

        public override string Message => "Command validation failed";
    }
}