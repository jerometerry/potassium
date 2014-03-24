namespace Potassium.Internal
{
    using System;

    internal static class Constants
    {
        /// <summary>
        /// Fine-grained lock that protects listeners and nodes. 
        /// </summary>
        internal static readonly object SubscriptionLock = new object();

        /// <summary>
        /// Coarse-grained lock that's held during the whole transaction. 
        /// </summary>
        internal static readonly object TransactionLock = new object();

        /// <summary>
        /// Lock that protects the value of providers during calls to the VAlue property
        /// </summary>
        internal static readonly object ProviderValueLock = new object();

        /// <summary>
        /// The amount of time to wait to acquire a lock, to detect deadlocks
        /// </summary>
        internal static readonly TimeSpan LockTimeout = TimeSpan.FromSeconds(1);
    }
}
