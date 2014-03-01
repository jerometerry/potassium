namespace Sodium
{
    using System;

    /// <summary>
    /// Base class for Event and Behavior
    /// </summary>
    public abstract class Observable<TA> : SodiumObject
    {
        public abstract IEventListener<TA> Listen(Action<TA> action);

        public abstract IEventListener<TA> ListenSuppressed(Action<TA> action);

        public abstract void Fire(TA a);
    }
}
