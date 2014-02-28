namespace Sodium
{
    using System;

    /// <summary>
    /// Base class for Events and Behaviors
    /// </summary>
    public class Observable : IDisposable
    {
        private static long sequence = 1;

        private readonly long id;

        protected Observable()
        {
            id = sequence++;
            Metrics.LiveObserables.Add(this);
            Metrics.ObservableAllocations++;
        }

        /// <summary>
        /// Gets / sets a description for the current Observable
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets whether the current Event has been disposed
        /// </summary>
        public bool Disposed { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public bool Disposingg { get; private set; }

        public override int GetHashCode()
        {
            return this.id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return this.id == ((Observable)obj).id;
        }

        /// <summary>
        /// Stop all listeners from receiving events from the current Event
        /// </summary>
        public void Dispose()
        {
            if (this.Disposed || this.Disposingg)
            {
                return;
            }

            Metrics.ObservableDeallocations++;
            Dispose(true);
            GC.SuppressFinalize(this);

            this.Disposed = true;
            this.Disposingg = false;

            Metrics.LiveObserables.Remove(this);
        }

        protected void AssertNotDisposed()
        {
            if (Disposed)
            {
                throw new ObjectDisposedException("Observable is being used after it's disposed");
            }
        }

        protected virtual void Dispose(bool disposing)
        {
        }
    }
}
