namespace Sodium.Continuous
{
    using Sodium.Core;
    using Sodium.Discrete;
    using System;

    /// <summary>
    /// ContinuousBehavior is the base class for Behaviors based on a continuous stream of values,
    /// such as the System Clock, or for values pulled lazily (when value is requested) 
    /// from external sources such as web services.
    /// </summary>
    /// <typeparam name="T">The type of value of the current Behavior</typeparam>
    public abstract class ContinuousBehavior<T> : DisposableObject, IBehavior<T>
    {
        /// <summary>
        /// Sample the Behaviors current value
        /// </summary>
        public abstract T Value { get; }

        public ContinuousBehavior<TB> Map<TB>(Func<T, TB> map)
        {
            return new ApplyBehavior<T, TB>(this, map);
        }

        public ContinuousBehavior<TB> Apply<TB>(IBehavior<Func<T, TB>> bf)
        {
            return new ApplyBehavior<T, TB>(this, bf);
        }

        public ContinuousBehavior<TC> Lift<TB,TC>(Func<T, TB, TC> lift, IBehavior<TB> b)
        {
            return new BinaryBehavior<T, TB, TC>(lift, this, b);
        }

        public ContinuousBehavior<TD> Lift<TB, TC, TD>(Func<T, TB, TC, TD> lift, IBehavior<TB> b, IBehavior<TC> c)
        {
            return new TernaryBehavior<T, TB, TC, TD>(lift, this, b, c);
        }

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
