namespace Sodium
{
    using System;

    public interface IValue<T> : IDisposable
    {
        T Value { get; }
    }
}
