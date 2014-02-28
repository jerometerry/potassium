namespace Sodium
{
    using System.Collections.Generic;

    /// <summary>
    /// Metrics for the Sodium assembly
    /// </summary>
    public static class Metrics
    {
        /// <summary>
        /// Get the number of times Events are constructed
        /// </summary>
        public static long EventAllocations { get; internal set; }
        
        /// <summary>
        /// Get the number of times Events are disposed
        /// </summary>
        public static long EventDeallocations { get; internal set; }

        public static long ItemAllocations { get; internal set; }

        /// <summary>
        /// Get the number of times Behaviors are constructed
        /// </summary>
        public static long BehaviorAllocations { get; internal set; }
        
        /// <summary>
        /// Get the number of times Behaviors are disposed
        /// </summary>
        public static long BehaviorDeallocations { get; internal set; }

        public static long ItemDeallocations { get; internal set; }

        /// <summary>
        /// Get the number of times Events are fired to their listeners
        /// </summary>
        public static long EventFirings { get; internal set; }

        public static HashSet<SodiumItem> LiveItems = new HashSet<SodiumItem>();

        public static long LiveItemCount 
        { 
            get
            {
                return LiveItems.Count;
            } 
        }

        /// <summary>
        /// Call the AutoDispose method on any live SodiumItems
        /// </summary>
        public static void AutoDispose()
        {
            var clone = new List<SodiumItem>(LiveItems);
            foreach(var o in clone)
            {
                o.AutoDispose();
            }

            LiveItems.Clear();
        }
    }
}