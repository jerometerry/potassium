namespace Sodium
{
    using System;

    public interface IListener<TA> : IDisposable
    {
        Event<TA> Event { get; }

        ICallback<TA> Action { get; }
    }
}