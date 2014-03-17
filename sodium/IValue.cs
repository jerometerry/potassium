namespace Sodium
{
    public interface IValue<T> : IEvent<T>
    {
        T Value { get; }
    }
}
