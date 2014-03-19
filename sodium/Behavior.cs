namespace Sodium
{
    using System;

    /// <summary>
    /// A Behavior is a time varying value. It starts with an initial value which 
    /// gets updated as the underlying Observable is published.
    /// </summary>
    /// <typeparam name="T">The type of values that will be published through the Behavior.</typeparam>
    public class Behavior<T> : Observable<T>
    {
        private ValueContainer<T> valueContainer;

        /// <summary>
        /// Create a behavior with a time varying value from an initial value
        /// </summary>
        /// <param name="initValue">The initial value of the Behavior</param>
        public Behavior(T initValue)
            : this(new Event<T>(), initValue)
        {
            this.Register(this.Source);
        }

        /// <summary>
        /// Create a behavior with a time varying value from an Event and an initial value
        /// </summary>
        /// <param name="source">The Observable to listen for updates from</param>
        /// <param name="initValue">The initial value of the Behavior</param>
        public Behavior(Observable<T> source, T initValue)
        {
            this.Source = source;
            this.valueContainer = new ValueContainer<T>(source, initValue);
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
                return this.valueContainer.Value;
            }
        }

        /// <summary>
        /// New value of the Behavior that will be posted to Value when the Transaction completes
        /// </summary>
        internal T NewValue
        {
            get
            {
                return this.valueContainer.NewValue;
            }
        }

        /// <summary>
        /// The underlying Observable of the current Behavior
        /// </summary>
        internal Observable<T> Source { get; private set; }

        /// <summary>
        /// Apply a value inside a behavior to a function inside a behavior. This is the
        /// primitive for all function lifting.
        /// </summary>
        /// <typeparam name="TB">The return type of the inner function of the given Behavior</typeparam>
        /// <param name="bf">Behavior of functions that maps from T -> TB</param>
        /// <returns>The new applied Behavior</returns>
        public Behavior<TB> Apply<TB>(Behavior<Func<T, TB>> bf)
        {
            return Transformer.Default.Apply(this, bf);
        }

        /// <summary>
        /// Transform a behavior with a generalized state loop (a mealy machine). The function
        /// is passed the input and the old state and returns the new state and output value.
        /// </summary>
        /// <typeparam name="TB">The type of the returned Behavior</typeparam>
        /// <typeparam name="TS">The snapshot function</typeparam>
        /// <param name="initState">Value to pass to the snapshot function</param>
        /// <param name="snapshot">Snapshot function</param>
        /// <returns>A new Behavior that collects values of type TB</returns>
        public Behavior<TB> Collect<TB, TS>(TS initState, Func<T, TS, Tuple<TB, TS>> snapshot)
        {
            return Transformer.Default.Collect(this, initState, snapshot);
        }

        /// <summary>
        /// Lift a binary function into a Behavior.
        /// </summary>
        /// <typeparam name="TB">The type of the given Behavior</typeparam>
        /// <typeparam name="TC">The return type of the lift function.</typeparam>
        /// <param name="lift">The function to lift, taking a T and a TB, returning TC</param>
        /// <param name="behavior">The behavior used to apply a partial function by mapping the given 
        /// lift method to the current Behavior.</param>
        /// <returns>A new Behavior who's value is computed using the current Behavior, the given
        /// Behavior, and the lift function.</returns>
        public Behavior<TC> Lift<TB, TC>(Func<T, TB, TC> lift, Behavior<TB> behavior)
        {
            return Transformer.Default.Lift(lift, this, behavior);
        }

        /// <summary>
        /// Lift a ternary function into a Behavior.
        /// </summary>
        /// <typeparam name="TB">Type of Behavior b</typeparam>
        /// <typeparam name="TC">Type of Behavior c</typeparam>
        /// <typeparam name="TD">Return type of the lift function</typeparam>
        /// <param name="lift">The function to lift</param>
        /// <param name="b">Behavior of type TB used to do the lift</param>
        /// <param name="c">Behavior of type TC used to do the lift</param>
        /// <returns>A new Behavior who's value is computed by applying the lift function to the current
        /// behavior, and the given behaviors.</returns>
        /// <remarks>Lift converts a function on values to a Behavior on values</remarks>
        public Behavior<TD> Lift<TB, TC, TD>(Func<T, TB, TC, TD> lift, Behavior<TB> b, Behavior<TC> c)
        {
            return Transformer.Default.Lift(lift, this, b, c);
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
            return Transformer.Default.Map(this, map);
        }

        /// <summary>
        /// Listen to the underlying event for updates, publishing the current value of the Behavior immediately.
        /// </summary>
        /// <param name="callback"> action to invoke when the underlying event publishes</param>
        /// <returns>The event subscription</returns>
        public ISubscription<T> SubscribeValues(Action<T> callback)
        {
            var evt = Transformer.Default.Values(this);
            var s = (Subscription<T>)evt.Subscribe(new Publisher<T>(callback), Rank.Highest);
            s.Register(evt);
            return s;
        }

        #region Subscribe Overrides
        
        /* Override Subscribe methods, forwarding subscription to the underlying Observable. */

        public override ISubscription<T> Subscribe(Action<T> callback)
        {
            return Source.Subscribe(callback);
        }

        internal override bool CancelSubscription(ISubscription<T> subscription)
        {
            return Source.CancelSubscription(subscription);
        }

        internal override ISubscription<T> Subscribe(Publisher<T> callback)
        {
            return Source.Subscribe(callback);
        }

        internal override ISubscription<T> Subscribe(Publisher<T> callback, Observable<T> superior)
        {
            return Source.Subscribe(callback, superior);
        }

        internal override ISubscription<T> Subscribe(Publisher<T> callback, Rank subscriptionRank)
        {
            return Source.Subscribe(callback, subscriptionRank);
        }

        internal override ISubscription<T> Subscribe(Publisher<T> callback, Rank superior, Transaction transaction)
        {
            return Source.Subscribe(callback, superior, transaction);
        }
        #endregion

        /// <summary>
        /// Dispose of the current Behavior
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (this.valueContainer != null)
            {
                this.valueContainer.Dispose();
                this.valueContainer = null;
            }

            base.Dispose(disposing);
        }
    }
}