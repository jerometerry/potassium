namespace JT.Rx.Net.Core
{
    using JT.Rx.Net.Continuous;
    using JT.Rx.Net.Discrete;
    using System;

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

        public ContinuousBehaviorDiscretizer<T> ToEvent(TimeSpan interval, Func<bool> until)
        {
            return new ContinuousBehaviorDiscretizer<T>(this, interval, until);
        }

        public ContinuousBehaviorDiscretizer<T> ToEvent(TimeSpan interval, PredicateBehavior until)
        {
            return new ContinuousBehaviorDiscretizer<T>(this, interval, until);
        }

        public EventDrivenBehavior<T> ToDiscreteBehavior(TimeSpan interval, Func<bool> until)
        {
            return ToDiscreteBehavior(interval, new QueryPredicateBehavior(until));
        }

        public EventDrivenBehavior<T> ToDiscreteBehavior(TimeSpan interval, PredicateBehavior until)
        {
            var evt = this.ToEvent(interval, until);
            var beh = evt.Hold(this.Value);
            beh.Register(evt);
            return beh;
        }
    }
}
