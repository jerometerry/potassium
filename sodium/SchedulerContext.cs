namespace Sodium
{
    using System;

    /// <summary>
    /// A SchedulerContext is responsible for obtaining a Scheduler
    /// used to run functions accepting a Scheduler as a parameter.
    /// </summary>
    public class SchedulerContext
    {
        private static readonly SchedulerContext CurrentContext = new SchedulerContext();

        private static Scheduler currentScheduler;

        /// <summary>
        /// Get the current SchedulerContext.
        /// </summary>
        public static SchedulerContext Current
        {
            get { return CurrentContext; }
        }

        /// <summary>
        /// Creates a new SchedulerTask
        /// </summary>
        /// <returns>The SchedulerTask</returns>
        public SchedulerTask<T> CreateTask<T>(Func<Scheduler, T> f)
        {
            var created = false;
            if (currentScheduler == null)
            {
                currentScheduler = new Scheduler();
                created = true;
            }

            return new SchedulerTask<T>
            {
                Context =  this,
                Scheduler = currentScheduler,
                Created = created,
                Task = f
            };
        }

        /// <summary>
        /// Complete the given task, running any scheduled actions if the associated scheduler
        /// was created for the given task
        /// </summary>
        /// <param name="request">The request to complete</param>
        public void CompleteTask<T>(SchedulerTask<T> request)
        {
            if (request == null)
            {
                return;
            }

            if (request.Created)
            {
                currentScheduler.Run();
                currentScheduler.Dispose();
                currentScheduler = null;
            }
        }
    }
}
