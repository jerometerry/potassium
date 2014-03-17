namespace Sodium
{
    using System;

    public interface IValue<T> : IObservable<T>
    {
        T Value { get; }
    }
}
