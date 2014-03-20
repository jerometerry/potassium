namespace Sodium
{
    using System;

    /// <summary>
    /// A Behavior is a continuous, time varying value. It starts with an initial value which 
    /// gets updated as the underlying Event is published.
    /// </summary>
    /// <typeparam name="T">The type of values that will be published through the Behavior.</typeparam>
    /// <remarks> In theory, a Behavior is a continuous value, whereas an Event is a discrete sequence of values.
    /// In Sodium.net, a Behavior is implemented by observing the discrete values of an Event, so a Behavior
    /// technically isn't continuous.
    /// </remarks>
    public class EventBasedBehavior<T> : ObservedValueBehavior<T>
    {
        /// <summary>
        /// Create a behavior with a time varying value from an initial value
        /// </summary>
        /// <param name="value">The initial value of the Behavior</param>
        public EventBasedBehavior(T value)
            : this(value, new Event<T>())
        {
            this.Register(this.Source);
        }

        /// <summary>
        /// Create a behavior with a time varying value from an Event and an initial value
        /// </summary>
        /// <param name="source">The Observable to listen for updates from</param>
        /// <param name="value">The initial value of the Behavior</param>
        public EventBasedBehavior(T value, Event<T> source)
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