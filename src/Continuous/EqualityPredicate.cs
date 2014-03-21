namespace JT.Rx.Net.Continuous
{
    using JT.Rx.Net.Core;
    
    public class EqualityPredicate<T> : Predicate
    {
        private IBehavior<T> behavior;
        private T value;

        public EqualityPredicate(IBehavior<T> behavior, T value)
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
