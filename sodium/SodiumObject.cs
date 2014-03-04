namespace Sodium
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Base class for Events, Behaviors, and Listeners
    /// </summary>
    public class SodiumObject : IDisposable
    {
        private List<SodiumObject> finalizers;

        /// <summary>
        /// Constructs a new SodiumObject
        /// </summary>
        protected SodiumObject()
        {
        }

        /// <summary>
        /// Gets whether the current SodiumObject is disposed
        /// </summary>
        public bool Disposed { get; private set; }
        
        /// <summary>
        /// Gets whether the current SodiumObject is being disposed.
        /// </summary>
        public bool Disposing { get; private set; }

        private SchedulerContext Context
        {
            get
            {
                return SchedulerContext.Current;
            }
        }

        /// <summary>
        /// Registers the given SodiumObject to be disposed when the current
        /// SodiumObject is disposed.
        /// </summary>
        /// <param name="o"></param>
        public void RegisterFinalizer(SodiumObject o)
        {
            if (finalizers == null)
            {
                finalizers = new List<SodiumObject>();
            }

            finalizers.Add(o);
        }

        /// <summary>
        /// Disposes the current SodiumObject
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.Disposed || this.Disposing)
            {
                return;
            }

            this.Disposing = true;

            if (finalizers != null && finalizers.Count > 0)
            {
                foreach (var item in finalizers)
                {
                    item.Dispose();
                }

                finalizers.Clear();
                finalizers = null;
            }

            this.Disposed = true;
            this.Disposing = false;
        }

        /// <summary>
        /// Run the given Function using a Scheduler obtained from ActionSchedulerContext.Current
        /// </summary>
        /// <typeparam name="TR">The return type of the Function</typeparam>
        /// <param name="f">The Function to run</param>
        /// <returns>The result of the Function</returns>
        protected TR RunScheduler<TR>(Func<Scheduler, TR> f)
        {
            lock (Constants.SchedulerLock)
            {
                using (var task = this.Context.CreateTask(f))
                {
                    return task.Run();
                }
            }
        }
    }
}
