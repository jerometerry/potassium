namespace Sodium
{
    using System;

    /// <summary>
    /// SingleTransactionContext is used to ensure that only a single Scheduler instance is created.
    /// </summary>
    /// <remarks>SingleTransactionContext.Run may be invoked recursively indirectly. Hence the
    /// need to ensure that the root ActionSchedulerContext.Current.Run is the only one to create
    /// a new Scheduler object.</remarks>
    public sealed class SingleActionSchedulerContext : ActionSchedulerContext
    {
        private static ActionScheduler current;

        /// <summary>
        /// Run the specified function inside a single scheduler
        /// </summary>
        /// <typeparam name="T">The return type of the function</typeparam>
        /// <param name="f">Function that accepts a scheduler and returns a value</param>
        /// <returns></returns>
        /// <remarks>
        /// In most cases this is not needed, because all APIs will create their own
        /// scheduler automatically. It is useful where you want to run multiple
        /// reactive operations atomically.
        /// </remarks>
        public override T Start<T>(Func<ActionScheduler, T> f)
        {
            lock (Constants.TransactionLock)
            {
                // If we are already inside a scheduler (which must be on the same
                // thread otherwise we wouldn't have acquired transactionLock), then
                // keep using that same scheduler.
                var created = false;
                if (current == null)
                {
                    current = new ActionScheduler();
                    created = true;
                }

                try
                {
                    return f(current);
                }
                finally
                {
                    if (created)
                    {
                        current.Run();
                        current.Dispose();
                        current = null;
                    }
                }
            }
        }
    }
}