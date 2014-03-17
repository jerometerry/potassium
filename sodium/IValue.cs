namespace Sodium
{
    public interface IValue<T> : IObservable<T>
    {
        T Value { get; }
    }
}
