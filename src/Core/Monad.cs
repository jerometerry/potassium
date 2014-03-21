namespace JT.Rx.Net.Core
{
    using System;
    using JT.Rx.Net.Continuous;
    using JT.Rx.Net.Discrete;

    /// <summary>
    /// ContinuousBehavior is the base class for Behaviors based on a continuous stream of values,
    /// such as the System Clock, or for values pulled lazily (when value is requested) 
    /// from external sources such as web services.
    /// </summary>
    /// <typeparam name="T">The type of value of the current Behavior</typeparam>
    public abstract class Monad<T> : DisposableObject, IBehavior<T>
    {
        /// <summary>
        /// Sample the Behaviors current value
        /// </summary>
        public abstract T Value { get; }

        public Discretizer<T> ToEvent(TimeSpan interval, Func<bool> until)
        {
            return new Discretizer<T>(this, interval, until);
        }

        public Discretizer<T> ToEvent(TimeSpan interval, Predicate until)
        {
            return new Discretizer<T>(this, interval, until);
        }

        public Behavior<T> ToDiscreteBehavior(TimeSpan interval, Func<bool> until)
        {
            return ToDiscreteBehavior(interval, new QueryPredicate(until));
        }

        public Behavior<T> ToDiscreteBehavior(TimeSpan interval, Predicate until)
        {
            var evt = this.ToEvent(interval, until);
            var beh = evt.Hold(this.Value);
            beh.Register(evt);
            return beh;
        }
    }
}
