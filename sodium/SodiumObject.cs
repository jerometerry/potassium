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
        /// Gets / sets a description for the current Observable
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets whether the current SodiumObject is disposed
        /// </summary>
        public bool Disposed { get; private set; }
        
        /// <summary>
        /// Gets whether the current SodiumObject is being disposed.
        /// </summary>
        public bool Disposing { get; private set; }

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
        public virtual void Dispose()
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
    }
}
