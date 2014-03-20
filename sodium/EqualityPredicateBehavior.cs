namespace Sodium
{
    public class EqualityPredicateBehavior<T> : PredicateBehavior
    {
        private Behavior<T> behavior;
        private T value;

        public EqualityPredicateBehavior(Behavior<T> behavior, T value)
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
