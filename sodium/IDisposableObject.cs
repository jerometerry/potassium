using System;
namespace Sodium
{
    public interface IDisposableObject : IDisposable
    {
        bool Disposed { get; }
        bool Disposing { get; }
        void RegisterFinalizer(IDisposable o);
    }
}
