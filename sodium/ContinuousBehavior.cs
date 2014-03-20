namespace Sodium
{
    using System;

    public abstract class ContinuousBehavior<T> : DisposableObject, IBehavior<T>
    {
        public abstract T Value { get; }

        public ContinuousBehavior<TB> Map<TB>(Func<T, TB> map)
        {
            return new MappedBehavior<T, TB>(this, map);
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

        public ContinuousBehaviorDiscretizer<T> Discretize(TimeSpan interval, Func<bool> until)
        {
            return new ContinuousBehaviorDiscretizer<T>(this, interval, until);
        }

        public ContinuousBehaviorDiscretizer<T> Discretize(TimeSpan interval, PredicateBehavior until)
        {
            return new ContinuousBehaviorDiscretizer<T>(this, interval, until);
        }
    }
}
