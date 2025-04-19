namespace CarbonWise.BuildingBlocks.Domain
{
    public interface IAggregateRoot
    {
    }
}

namespace CarbonWise.BuildingBlocks.Domain
{
    public interface IBusinessRule
    {
        bool IsBroken();
        string Message { get; }
    }
}

namespace CarbonWise.BuildingBlocks.Domain
{
    public interface IDomainEvent
    {
    }
}
namespace CarbonWise.BuildingBlocks.Domain
{
    public abstract class DomainEventBase : IDomainEvent
    {
        public Guid Id { get; }

        public DateTime OccurredOn { get; }

        protected DomainEventBase()
        {
            Id = Guid.NewGuid();
            OccurredOn = DateTime.UtcNow;
        }
    }
}

namespace CarbonWise.BuildingBlocks.Domain
{
    public class BusinessRuleValidationException : Exception
    {
        public IBusinessRule BrokenRule { get; }

        public BusinessRuleValidationException(IBusinessRule brokenRule) : base(brokenRule.Message)
        {
            BrokenRule = brokenRule;
        }

        public override string ToString()
        {
            return $"{BrokenRule.GetType().Name}: {Message}";
        }
    }
}

namespace CarbonWise.BuildingBlocks.Domain
{
    // See: https://enterprisecraftsmanship.com/posts/value-object-better-implementation/
    public abstract class ValueObject : IEquatable<ValueObject>
    {
        public bool Equals(ValueObject other)
        {
            return other is not null && ValuesAreEqual(other);
        }

        public override bool Equals(object obj)
        {
            return obj is ValueObject other && ValuesAreEqual(other);
        }

        public override int GetHashCode()
        {
            return GetEqualityComponents()
                .Aggregate(1, (current, obj) =>
                {
                    unchecked
                    {
                        return current * 23 + (obj?.GetHashCode() ?? 0);
                    }
                });
        }

        protected abstract IEnumerable<object> GetEqualityComponents();

        private bool ValuesAreEqual(ValueObject other)
        {
            return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
        }

        public static bool operator ==(ValueObject left, ValueObject right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ValueObject left, ValueObject right)
        {
            return !Equals(left, right);
        }
    }
}

namespace CarbonWise.BuildingBlocks.Domain
{
    public abstract class TypedIdValueBase : IEquatable<TypedIdValueBase>
    {
        public Guid Value { get; }

        protected TypedIdValueBase(Guid value)
        {
            Value = value;
        }

        public bool Equals(TypedIdValueBase other)
        {
            return other is not null && Value == other.Value && GetType() == other.GetType();
        }

        public override bool Equals(object obj)
        {
            return obj is TypedIdValueBase other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public static bool operator ==(TypedIdValueBase left, TypedIdValueBase right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(TypedIdValueBase left, TypedIdValueBase right)
        {
            return !Equals(left, right);
        }
    }
}

namespace CarbonWise.BuildingBlocks.Domain
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class IgnoreMemberAttribute : Attribute
    {
    }
}