namespace Sodium
{
    using System;

    /// <summary>
    /// Base class for Event and Behavior
    /// </summary>
    public abstract class Observable<T> : SodiumObject
    {
        public abstract IEventListener<T> Listen(Action<T> action);

        public abstract IEventListener<T> ListenSuppressed(Action<T> action);

        public abstract void Fire(T a);
    }
}
