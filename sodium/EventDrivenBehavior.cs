namespace Sodium
{
    /// <summary>
    /// EventBasedBehavior is a Behavior who's value is updated when the underlying Event is updated.
    /// </summary>
    /// <typeparam name="T">The type of values that will be published through the Behavior.</typeparam>
    public class EventDrivenBehavior<T> : ObservableDrivenBehavior<T>
    {
        /// <summary>
        /// Create a behavior with a time varying value from an initial value
        /// </summary>
        /// <param name="value">The initial value of the Behavior</param>
        public EventDrivenBehavior(T value)
            : this(value, new Event<T>())
        {
            this.Register(this.Source);
        }

        /// <summary>
        /// Create a behavior with a time varying value from an Event and an initial value
        /// </summary>
        /// <param name="source">The Observable to listen for updates from</param>
        /// <param name="value">The initial value of the Behavior</param>
        public EventDrivenBehavior(T value, Event<T> source)
            : base(source, value)
        {
            this.Source = source;
        }

        /// <summary>
        /// The underlying Event of the current Behavior
        /// </summary>
        public Event<T> Source { get; private set; }
    }
}