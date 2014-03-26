namespace Potassium.Core
{
    using System;
    using Potassium.Internal;
    using Potassium.Providers;

    /// <summary>
    /// Behavior contains a value which is updated when the underlying Event is updated.
    /// </summary>
    /// <typeparam name="T">The type of values that will be fired through the Behavior.</typeparam>
    public class Behavior<T> : Provider<T>
    {
        private ObservedValue<T> observedValue;

        /// <summary>
        /// Creates a constant Behavior
        /// </summary>
        /// <param name="value">The value of the Behavior</param>
        public Behavior(T value)
            : this(value, new Event<T>())
        {
            this.Register(this.Source);
        }

        /// <summary>
        /// Create a behavior with a time varying value from an Event and an initial value
        /// </summary>
        /// <param name="source">The Observable to listen for updates from</param>
        /// <param name="value">The initial value of the Behavior</param>
        public Behavior(T value, Event<T> source)
        {
            this.Source = source;
            this.observedValue = Transaction.Start(t => new ObservedValue<T>(value, source, t));
        }

        /// <summary>
        /// The underlying Event of the current Behavior
        /// </summary>
        public Event<T> Source { get; private set; }

        public override T Value
        {
            get
            {
                return observedValue.Value;
            }
        }

        /// <summary>
        /// New value of the Behavior that will be posted to Value when the Transaction completes
        /// </summary>
        internal T NewValue
        {
            get
            {
                return this.observedValue.NewValue;
            }
        }

        /// <summary>
        /// Transform the behavior's value according to the supplied function.
        /// </summary>
        /// <typeparam name="TB">The return type of the mapping function</typeparam>
        /// <param name="map">The mapping function that converts from T -> TB</param>
        /// <returns>A new Behavior that updates whenever the current Behavior updates,
        /// having a value computed by the map function, and starting with the value
        /// of the current event mapped.</returns>
        public Behavior<TB> Map<TB>(Func<T, TB> map)
        {
            var mapEvent = this.Source.Map(map);
            var behavior = mapEvent.Hold(map(this.Value));
            behavior.Register(mapEvent);
            return behavior;
        }

        /// <summary>
        /// Get an Event that fires the initial Behaviors value, and whenever the Behaviors value changes.
        /// </summary>
        /// <typeparam name="T">The type of values fired through the source</typeparam>
        /// <returns>The event streams</returns>
        public Event<T> Values()
        {
            return Transaction.Start(t => new BehaviorLastValueEvent<T>(this, t));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                observedValue.Dispose();
                observedValue = null;
            }

            base.Dispose(disposing);
        }
    }
}