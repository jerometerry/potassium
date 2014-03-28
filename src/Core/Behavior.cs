namespace Potassium.Core
{
    using System;
    using Potassium.Internal;
    using Potassium.Providers;
    using Potassium.Utilities;

    /// <summary>
    /// Behavior is a time varying value. A Behavior starts with an initial value which gets updated when the underlying Event is fired.
    /// </summary>
    /// <typeparam name="T">The type of values that will be fired through the Behavior.</typeparam>
    public class Behavior<T> : Provider<T>, IObservable<T>
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
        /// Evaluates the value of the Provider
        /// </summary>
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
        /// The underlying Event of the current Behavior
        /// </summary>
        private Event<T> Source { get; set; }

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

        public ISubscription<T> Subscribe(Action<T> callback)
        {
            return this.Source.Subscribe(callback);
        }

        public ISubscription<T> SubscribeValues(Action<T> callback)
        {
            var vals = this.Values();
            var s = (Subscription<T>)vals.Subscribe(callback);
            s.Register(vals);
            return s;
        }

        internal Event<T> LastFiring()
        {
            return this.Source.LastFiring();
        }

        internal ISubscription<T> Subscribe(Observer<T> observer, Priority subscriptionRank)
        {
            return this.Source.Subscribe(observer, subscriptionRank);
        }

        internal ISubscription<T> Subscribe(Observer<T> observer, Priority superior, Transaction transaction)
        {
            return this.Source.Subscribe(observer, superior, transaction);
        }

        internal ISubscription<T> SubscribeValues(Observer<T> observer, Priority subscriptionRank)
        {
            var vals = this.Values();
            var s = (Subscription<T>)vals.Subscribe(observer, subscriptionRank);
            s.Register(vals);
            return s;
        }

        internal ISubscription<T> SubscribeValues(Observer<T> observer, Priority superior, Transaction transaction)
        {
            var vals = this.Values();
            var s = (Subscription<T>)vals.Subscribe(observer, superior, transaction);
            s.Register(vals);
            return s;
        }

        protected void RegisterSource()
        {
            this.Register(this.Source);
        }

        /// <summary>
        /// Clean up all resources used by the current SodiumObject
        /// </summary>
        /// <param name="disposing">Whether to dispose managed resources
        /// </param>
        protected override void Dispose(bool disposing)
        {
            if (observedValue != null)
            {
                observedValue.Dispose();
                observedValue = null;
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Get an Event that fires the initial Behaviors value, and whenever the Behaviors value changes.
        /// </summary>
        /// <returns>The event streams</returns>
        private Event<T> Values()
        {
            return Transaction.Start(t => new BehaviorLastValueEvent<T>(this, t));
        }
    }
}