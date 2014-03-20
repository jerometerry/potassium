namespace Sodium
{
    using System;

    /// <summary>
    /// A Behavior is a time varying value. It starts with an initial value which 
    /// gets updated as the underlying Event is published.
    /// </summary>
    /// <typeparam name="T">The type of values that will be published through the Behavior.</typeparam>
    /// <remarks>A Behavior contains an Event, maintains the last value published from the Event,
    /// and has several transformation methods to constructs other Behaviors and Events.
    /// 
    /// A Behavior is composed of an Event (i.e. has-a). 
    /// 
    /// A Behavior is not an Event. Should it be?
    /// </remarks>
    public class Behavior<T> : DisposableObject
    {
        /// <summary>
        /// Create a behavior with a time varying value from an initial value
        /// </summary>
        /// <param name="value">The initial value of the Behavior</param>
        public Behavior(T value)
            : this(new Event<T>(), value)
        {
            this.Register(this.Source);
        }

        /// <summary>
        /// Create a behavior with a time varying value from an Event and an initial value
        /// </summary>
        /// <param name="source">The Observable to listen for updates from</param>
        /// <param name="value">The initial value of the Behavior</param>
        public Behavior(Event<T> source, T value)
        {
            this.Source = source;
            this.ObservedValue = new ObservedValue<T>(source, value);
        }

        /// <summary>
        /// Sample the behavior's current value
        /// </summary>
        /// <remarks>
        /// This should generally be avoided in favor of SubscribeValues(..) so you don't
        /// miss any updates, but in many circumstances it makes sense.
        /// 
        /// Value is the value of the Behavior at the start of a Transaction
        ///
        /// It can be best to use it inside an explicit transaction (using TransactionContext.Current.Run()).
        /// For example, a b.Value inside an explicit transaction along with a
        /// b.Source.Subscribe(..) will capture the current value and any updates without risk
        /// of missing any in between.
        /// </remarks>
        public T Value
        {
            get
            {
                return this.ObservedValue.Value;
            }
        }

        /// <summary>
        /// New value of the Behavior that will be posted to Value when the Transaction completes
        /// </summary>
        internal T NewValue
        {
            get
            {
                return this.ObservedValue.NewValue;
            }
        }

        /// <summary>
        /// The underlying Event of the current Behavior
        /// </summary>
        internal Observable<T> Source { get; private set; }

        private ObservedValue<T> ObservedValue { get; set; }

        /// <summary>
        /// Listen to the underlying event for updates, publishing the current value of the Behavior immediately.
        /// </summary>
        /// <param name="callback"> action to invoke when the underlying event publishes</param>
        /// <returns>The event subscription</returns>
        public ISubscription<T> SubscribeValues(Action<T> callback)
        {
            var evt = this.Values();
            var s = (Subscription<T>)evt.Subscribe(new Publisher<T>(callback), Rank.Highest);
            s.Register(evt);
            return s;
        }

        /// <summary>
        /// Subscribe to publications of the current Observable.
        /// </summary>
        /// <param name="callback">An Action to be invoked when the current Observable publishes values.</param>
        /// <returns>An ISubscription, that should be Disposed when no longer needed. </returns>
        public ISubscription<T> Subscribe(Action<T> callback)
        {
            return this.Source.Subscribe(callback);
        }

        /// <summary>
        /// Dispose of the current Behavior
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (this.ObservedValue != null)
            {
                this.ObservedValue.Dispose();
                this.ObservedValue = null;
            }

            base.Dispose(disposing);
        }
    }
}