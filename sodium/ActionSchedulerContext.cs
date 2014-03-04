namespace Sodium
{
    using System;

    /// <summary>
    /// A ActionSchedulerContext is responsible for obtaining a Scheduler
    /// used to run functions accepting a Scheduler as a parameter.
    /// </summary>
    public abstract class ActionSchedulerContext
    {
        private static ActionSchedulerContext current = new SingleActionSchedulerContext();

        /// <summary>
        /// Get / set the current ActionSchedulerContext. The Default ActionSchedulerContext 
        /// is a SingleTransactionContext.
        /// </summary>
        public static ActionSchedulerContext Current
        {
            get { return current; }
            set { current = value; }
        }

        /// <summary>
        /// Run the given function passing in a located Scheduler
        /// </summary>
        /// <typeparam name="T">The return type of the supplied function</typeparam>
        /// <param name="f">The function to run</param>
        /// <returns>The return value of the supplied function</returns>
        public abstract T Start<T>(Func<ActionScheduler, T> f);
    }
}
