namespace Sodium
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Base class for Events, Behaviors, and Listeners
    /// </summary>
    public class SodiumItem : IDisposable
    {
        private List<SodiumItem> finalizers;

        protected SodiumItem()
        {
        }

        /// <summary>
        /// Gets / sets a description for the current Observable
        /// </summary>
        public string Description { get; set; }

        public void RegisterFinalizer(SodiumItem item)
        {
            if (finalizers == null)
            {
                finalizers = new List<SodiumItem>();
            }

            finalizers.Add(item);
        }

        public virtual void Dispose()
        {
            if (finalizers != null && finalizers.Count > 0)
            {
                var clone = new List<SodiumItem>(finalizers);
                finalizers.Clear();
                finalizers = null;

                foreach(var item in clone)
                {
                    item.Dispose();
                }
            }
        }
    }
}
