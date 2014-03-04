namespace Sodium
{
    using System;

    /// <summary>
    /// SchedulerTask runs a supplied function against the current Scheduler.
    /// </summary>
    public class SchedulerTask<T> : SodiumObject
    {
        /// <summary>
        /// The Scheduler to run the Task against
        /// </summary>
        public Scheduler Scheduler { get; set; }

        /// <summary>
        /// The SchedulerContext that created the Scheduler
        /// </summary>
        public SchedulerContext Context { get; set; }

        /// <summary>
        /// Whether a new Scheduler was created for this task
        /// </summary>
        public bool Created { get; set; }

        /// <summary>
        /// Get / set the current task to execute.
        /// </summary>
        public Func<Scheduler, T> Task { get; set; }

        /// <summary>
        /// Run the given function passing in a located Scheduler
        /// </summary>
        /// <typeparam name="T">The return type of the supplied function</typeparam>
        /// <param name="f">The function to run</param>
        /// <returns>The return value of the supplied function</returns>
        public T Run()
        {
            return Task(this.Scheduler);
        }

        protected override void Dispose(bool disposing)
        {
            this.Context.CompleteTask(this);
            base.Dispose(disposing);
        }
    }
}