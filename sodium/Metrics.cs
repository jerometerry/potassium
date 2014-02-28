namespace Sodium
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Metrics for the Sodium assembly
    /// </summary>
    public static class Metrics
    {
        private static readonly HashSet<SodiumItem> LiveItems = new HashSet<SodiumItem>();

        /// <summary>
        /// Get the number of times Events are constructed
        /// </summary>
        public static long EventAllocations { get; internal set; }
        
        /// <summary>
        /// Get the number of times Events are disposed
        /// </summary>
        public static long EventDeallocations { get; internal set; }

        /// <summary>
        /// Get the number of times Behaviors are constructed
        /// </summary>
        public static long BehaviorAllocations { get; internal set; }
        
        /// <summary>
        /// Get the number of times Behaviors are disposed
        /// </summary>
        public static long BehaviorDeallocations { get; internal set; }

        /// <summary>
        /// Get the number of times a SodiumItem has been created
        /// </summary>
        public static long ItemAllocations { get; internal set; }

        /// <summary>
        /// Get the number of times a SodiumItem has been disposed
        /// </summary>
        public static long ItemDeallocations { get; internal set; }

        /// <summary>
        /// Get the number of times Events are fired to their listeners
        /// </summary>
        public static long EventFirings { get; internal set; }

        /// <summary>
        /// Gets the number of SodiumItems that have been created but have not net been disposed
        /// </summary>
        public static long LiveItemCount 
        { 
            get
            {
                return LiveItems.Count;
            } 
        }

        /// <summary>
        /// Get the list of items created that have not been disposed.
        /// </summary>
        /// <returns>An IEnumerable of live items</returns>
        public static SodiumItem[] GetLiveItems()
        {
            return LiveItems.ToArray();
        }

        internal static void ItemAllocated(SodiumItem item)
        {
            LiveItems.Add(item);
        }

        internal static void ItemDeallocated(SodiumItem item)
        {
            LiveItems.Remove(item);
        }
    }
}