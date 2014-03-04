namespace Sodium
{
    internal static class Constants
    {
        /// <summary>
        /// Fine-grained lock that protects listeners and nodes. 
        /// </summary>
        internal static readonly object ListenersLock = new object();

        /// <summary>
        /// Coarse-grained lock that's held during the whole transaction. 
        /// </summary>
        internal static readonly object SchedulerLock = new object();
    }
}
