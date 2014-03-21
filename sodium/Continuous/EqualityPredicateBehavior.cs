namespace Sodium.Continuous
{
    using Sodium.Core;

    /// <summary>
    /// A Behavior with a boolean type (i.e. predicate), that compares
    /// the current value of a source Behavior with a static boolean value.
    /// </summary>
    /// <typeparam name="T">The type of value stored in the source Behavior</typeparam>
    public class EqualityPredicateBehavior<T> : PredicateBehavior
    {
        private IBehavior<T> behavior;
        private T value;

        public EqualityPredicateBehavior(IBehavior<T> behavior, T value)
        {
            this.behavior = behavior;
            this.value = value;
        }

        public override bool Value
        {
            get
            {
                var v1 = new Maybe<T>(behavior.Value);
                var v2 = new Maybe<T>(value);
                return v1.Equals(v2);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                behavior = null;
                value = default(T);
            }

            base.Dispose(disposing);
        }
    }
}
