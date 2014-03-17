namespace Sodium
{
    using System;

    /// <summary>
    /// ISnapshot is an Observable that can be take snapshots
    /// </summary>
    /// <typeparam name="T">The type of value fired through the Observable</typeparam>
    public interface ISnapshot<T> : IDisposable
    {
        
    }
}
