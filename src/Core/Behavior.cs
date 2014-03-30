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
        private Event<T> source;

        /// <summary>
        /// Creates a constant Behavior
        /// </summary>
        /// <param name="value">The value of the Behavior</param>
        public Behavior(T value)
        {
            this.Observe(new Event<T>(), value);
        }

        /// <summary>
        /// Create a behavior with a time varying value from an Event and an initial value
        /// </summary>
        /// <param name="source">The Observable to listen for updates from</param>
        /// <param name="value">The initial value of the Behavior</param>
        public Behavior(T value, Event<T> source)
        {
            this.Observe(source.Repeat(), value);
        }

        /// <summary>
        /// Initializes a new instance of the Behavior class.
        /// </summary>
        /// <remarks>Caller assumes responsibility for calling Observes</remarks>
        protected Behavior()
        {
        }

        /// <summary>
        /// Samples the value of the Behavior
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
        /// Creates a new Behavior with the Value of current Behavior, that updates 
        /// whenever the current Behavior updates
        /// </summary>
        /// <returns>The clone</returns>
        public Behavior<T> Clone()
        {
            return new Behavior<T>(this.Value, this.source);
        }

        /// <summary>
        /// Combine two Behavior's values together
        /// </summary>
        /// <param name="behavior">The behavior to combine with the current Behavior</param>
        /// <param name="combine">The combining function, to combine the values of the two behaviors together</param>
        /// <returns>A new Behavior who's value is computed from the current and given Behaviors, using the combining function</returns>
        /// <typeparam name="TB">The type of values in the given Behavior</typeparam>
        /// <typeparam name="TC">The return type of the combine function</typeparam>
        public Behavior<TC> Combine<TB, TC>(Behavior<TB> behavior, Func<T, TB, TC> combine)
        {
            // Listen for firings of the Behaviors. Don't care what the fired values are, 
            // since the Behaviors hold their updated values.
            var m1 = this.source.Map(t => Maybe<T>.Nothing);
            var m2 = behavior.source.Map(t => Maybe<T>.Nothing);

            // Merge the firings of the underlying Events mapped to nothing into a single event
            var merged = m1 | m2;

            // We're only interested in the last firing of the merged Event
            var last = merged.LastFiring();

            // Create a new Event that fires the combined new values of the two behaviors.
            // Use NewValue because Value isn't updated until the Transaction completes.
            var mapped = last.Map(t => combine(this.NewValue, behavior.NewValue));

            // Convert mapped into a Behavior, starting the the combined initial values 
            // of the two Behaviors
            var b = mapped.Hold(combine(this.Value, behavior.Value));

            // Register created events for cleanup
            b.Register(mapped);
            b.Register(last);
            b.Register(merged);
            b.Register(m1);
            b.Register(m2);
            return b;
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
            var mapEvent = this.source.Map(map);
            var behavior = mapEvent.Hold(map(this.Value));
            behavior.Register(mapEvent);
            return behavior;
        }

        /// <summary>
        /// Subscribe to updates of the Behavior's value
        /// </summary>
        /// <param name="callback">The callback method to receive notifications of value changes</param>
        /// <returns>The subscription</returns>
        public ISubscription<T> Subscribe(Action<T> callback)
        {
            return this.source.Subscribe(callback);
        }

        /// <summary>
        /// Same as Subscribe, but fires the Behaviors current value immediately
        /// </summary>
        /// <param name="callback">The callback method to receive notifications of value changes</param>
        /// <returns>The subscription</returns>
        public ISubscription<T> SubscribeWithInitialFire(Action<T> callback)
        {
            var vals = this.ToEventWithInitialFire();
            var s = (Subscription<T>)vals.Subscribe(callback);
            s.Register(vals);
            return s;
        }

        /// <summary>
        /// Create an Event that fires whenever the Behavior updates.
        /// </summary>
        /// <returns>An Event that fires whenever the underlying Event fires</returns>
        public Event<T> ToEvent()
        {
            return this.source.Repeat();
        }

        /// <summary>
        /// Creates an Event that fires the Behaviors initial value, and whenever the Behaviors underlying Event fires.
        /// </summary>
        /// <returns>The event streams</returns>
        public Event<T> ToEventWithInitialFire()
        {
            return Transaction.Start(t => new BehaviorLastValueEvent<T>(this, t));
        }

        internal Event<T> LastFiring()
        {
            return this.source.LastFiring();
        }

        internal ISubscription<T> Subscribe(Observer<T> observer, Priority subscriptionRank)
        {
            return this.source.Subscribe(observer, subscriptionRank);
        }

        internal ISubscription<T> Subscribe(Observer<T> observer, Priority superior, Transaction transaction)
        {
            return this.source.Subscribe(observer, superior, transaction);
        }

        internal ISubscription<T> SubscribeWithInitialFire(Observer<T> observer, Priority subscriptionRank)
        {
            var vals = this.ToEventWithInitialFire();
            var s = (Subscription<T>)vals.Subscribe(observer, subscriptionRank);
            s.Register(vals);
            return s;
        }

        internal ISubscription<T> SubscribeWithInitialFire(Observer<T> observer, Priority superior, Transaction transaction)
        {
            var vals = this.ToEventWithInitialFire();
            var s = (Subscription<T>)vals.Subscribe(observer, superior, transaction);
            s.Register(vals);
            return s;
        }

        /// <summary>
        /// Listen to the source for updates
        /// </summary>
        /// <param name="src">The source</param>
        /// <param name="value">The initial value of the Behavior</param>
        /// <remarks>The source Event will be disposed when the current Behavior is disposed. </remarks>
        protected void Observe(Event<T> src, T value)
        {
            if (this.source != null)
            {
                throw new ApplicationException("The Behavior is already observing");
            }

            this.source = src;
            this.observedValue = Transaction.Start(t => new ObservedValue<T>(value, this.source, t));
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

            if (this.source != null)
            {
                this.source.Dispose();
                this.source = null;
            }

            base.Dispose(disposing);
        }
    }
}