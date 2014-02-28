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

        public SodiumItem Originator { get; set; }

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
        /// Gets whether the current SodiumItem is being disposed
        /// </summary>
        public bool Disposing { get; private set; }

        /// <summary>
        /// Gets whether the current SodiumItem is being auto disposed
        /// </summary>
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
            if (this.Disposed || this.Disposing)
            {
                return;
            }

            this.Disposing = true;
            Metrics.ItemDeallocations++;
            Dispose(true);
            GC.SuppressFinalize(this);

            this.Disposed = true;
            this.Disposing = false;

            Metrics.LiveItems.Remove(this);
        }

        /// <summary>
        /// Dispose of the current SodiumItem, if AllowAutoDispose is enabled.
        /// </summary>
        public void AutoDispose()
        {
            if (!this.AllowAutoDispose || this.Disposed || this.Disposing || this.AutoDisposing)
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
