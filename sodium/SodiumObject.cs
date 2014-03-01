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

        protected SodiumObject()
        {
        }

        /// <summary>
        /// Gets / sets a description for the current Observable
        /// </summary>
        public string Description { get; set; }

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

        public virtual void Dispose()
        {
            if (finalizers != null && finalizers.Count > 0)
            {
                var clone = new List<SodiumObject>(finalizers);
                finalizers.Clear();
                finalizers = null;

                foreach (var item in clone)
                {
                    item.Dispose();
                }
            }
        }
    }
}
