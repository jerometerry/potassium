namespace Sodium
{
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

        /// <summary>
        /// Get the number of times Behaviors are constructed
        /// </summary>
        public static long BehaviorAllocations { get; internal set; }
        
        /// <summary>
        /// Get the number of times Behaviors are disposed
        /// </summary>
        public static long BehaviorDeallocations { get; internal set; }

        /// <summary>
        /// Get the number of times Events are fired to their listeners
        /// </summary>
        public static long EventFirings { get; internal set; }
    }
}