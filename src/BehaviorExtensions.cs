namespace JT.Rx.Net
{
    

    /// <summary>
    /// Reactive extensions for Behaviors
    /// </summary>
    public static class BehaviorExtensions
    {
        /// <summary>
        /// Unwrap a behavior inside another behavior to give a time-varying behavior implementation.
        /// </summary>
        /// <typeparam name="T">The type of values published through the source</typeparam>
        /// <param name="source">The Behavior with an inner Behavior to unwrap.</param>
        /// <returns>The new, unwrapped Behavior</returns>
        /// <remarks>Switch allows the reactive network to change dynamically, using 
        /// reactive logic to modify reactive logic.</remarks>
        public static Behavior<T> Switch<T>(this Behavior<Behavior<T>> source)
        {
            var value = source.Value.Value;
            var sink = new SwitchBehaviorEvent<T>(source);
            var result = sink.Hold(value);
            result.Register(sink);
            return result;
        }

        /// <summary>
        /// Unwrap an event inside a behavior to give a time-varying event implementation.
        /// </summary>
        /// <typeparam name="T">The type of values published through the source</typeparam>
        /// <param name="behavior">The behavior that wraps the event</param>
        /// <returns>The unwrapped event</returns>
        /// <remarks>TransactionContext.Current.Run is used to invoke the overload of the 
        /// UnwrapEvent operation that takes a thread. This ensures that any other
        /// actions triggered during UnwrapEvent requiring a transaction all get the same instance.
        /// 
        /// Switch allows the reactive network to change dynamically, using 
        /// reactive logic to modify reactive logic.
        /// </remarks>
        public static Event<T> Switch<T>(this Behavior<Event<T>> behavior)
        {
            return new SwitchEvent<T>(behavior);
        }
    }
}
