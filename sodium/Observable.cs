namespace Sodium
{
    using System;

    /// <summary>
    /// Base class for Events and Behaviors
    /// </summary>
    public class Observable : IDisposable
    {
        private bool isDisposed;
        private bool isDisposing;

        /// <summary>
        /// Stop all listeners from receiving events from the current Event
        /// </summary>
        public void Dispose()
        {
            //Metrics.EventDeallocations++;
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Gets whether the current Event has been disposed
        /// </summary>
        public bool Disposed
        {
            get { return isDisposed; }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool Disposingg
        {
            get { return isDisposing; }
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
            if (isDisposed)
            {
                return;
            }

            disposing = true;

            if (disposing)
            {
            }

            isDisposed = true;
            isDisposing = false;
        }
    }
}
