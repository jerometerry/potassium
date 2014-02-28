namespace Sodium
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Base class for Events, Behaviors, and Listeners
    /// </summary>
    public class SodiumItem : IDisposable
    {
        private static long sequence = 1;

        /// <summary>
        /// List of items that will be auto disposed of when the current SodiumItem is disposed
        /// </summary>
        private List<SodiumItem> autoFinalizers;

        /// <summary>
        /// List of items that will be disposed of when the current SodiumItem is disposed
        /// </summary>
        private List<SodiumItem> finalizers;

        protected SodiumItem(bool allowAutoDispose)
        {
            this.Id = sequence++;
            this.autoFinalizers = new List<SodiumItem>();
            this.finalizers = new List<SodiumItem>();
            Metrics.ItemAllocated(this);
            AllowAutoDispose = allowAutoDispose;
            Metrics.ItemAllocations++;
        }

        public long Id { get; private set; }

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
            return this.Id.GetHashCode();
        }

        public void RegisterAutoFinalizer(SodiumItem item)
        {
            this.autoFinalizers.Add(item);
        }

        public void RegisterFinalizer(SodiumItem item)
        {
            this.finalizers.Add(item);
        }

        public override bool Equals(object obj)
        {
            return this.Id == ((SodiumItem)obj).Id;
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

            Metrics.ItemDeallocated(this);
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
            if (!disposing)
            {
                return;
            }

            if (this.autoFinalizers != null && this.autoFinalizers.Count > 0)
            {
                var clone = new List<SodiumItem>(this.autoFinalizers);
                this.autoFinalizers.Clear();
                this.autoFinalizers = null;
                foreach (var item in clone)
                {
                    item.AutoDispose();
                }
            }

            if (this.finalizers != null && this.finalizers.Count > 0)
            {
                var clone = new List<SodiumItem>(this.finalizers);
                this.finalizers.Clear();
                this.finalizers = null;
                foreach (var item in clone)
                {
                    item.Dispose();
                }
            }
        }
    }
}
