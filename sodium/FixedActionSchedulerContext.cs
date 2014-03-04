namespace Sodium
{
    using System;

    /// <summary>
    /// FixedTransactionContext is a ActionSchedulerContext that uses a single (fixed) Scheduler
    /// throughout it's lifetime.
    /// </summary>
    public sealed class FixedActionSchedulerContext : ActionSchedulerContext, IDisposable
    {
        private ActionScheduler scheduler;
        private bool disposed;

        /// <summary>
        /// Creates a new FixedTransactionContext
        /// </summary>
        /// <param name="scheduler">The fixed Scheduler</param>
        public FixedActionSchedulerContext(ActionScheduler scheduler)
        {
            this.scheduler = scheduler;
        }

        /// <summary>
        /// Run the given function on the fixed scheduler
        /// </summary>
        /// <typeparam name="T">The return value of the supplied function</typeparam>
        /// <param name="f">The function to run on the fixed Scheduler</param>
        /// <returns>The result of calling the specified function</returns>
        public override T Start<T>(Func<ActionScheduler, T> f)
        {
            lock (Constants.SchedulerLock)
            {
                try
                {
                    return f(this.scheduler);
                }
                finally
                {
                    scheduler.Run();
                }
            }
        }

        /// <summary>
        /// Releases the current FixedTransactionContext, and it's fixed Scheduler.
        /// </summary>
        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            if (this.scheduler != null)
            {
                this.scheduler.Dispose();
                this.scheduler = null;
            }

            this.disposed = true;
        }
    }
}