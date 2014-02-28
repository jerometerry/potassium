namespace Sodium
{
    using System;

    /// <summary>
    /// Base class for Events, Behaviors, and Listeners
    /// </summary>
    public class SodiumItem : IDisposable
    {
        private static long sequence = 1;

        private readonly long id;

        protected SodiumItem()
        {
            id = sequence++;
            Metrics.LiveItems.Add(this);
            AllowAutoDispose = true;
            Metrics.ItemAllocations++;
        }

        public bool AllowAutoDispose { get; set; }

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

        public bool AutoDisposing { get; private set; }

        public override int GetHashCode()
        {
            return this.id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return this.id == ((SodiumItem)obj).id;
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

            Metrics.ItemDeallocations++;
            Dispose(true);
            GC.SuppressFinalize(this);

            this.Disposed = true;
            this.Disposingg = false;

            Metrics.LiveItems.Remove(this);
        }

        /// <summary>
        /// Dispose of the current SodiumItem, if AllowAutoDispose is enabled.
        /// </summary>
        public void AutoDispose()
        {
            if (!this.AllowAutoDispose)
            {
                return;
            }

            this.AutoDisposing = true;
            this.Dispose();
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
