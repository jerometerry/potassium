namespace JT.Rx.Net.Monads
{
    using JT.Rx.Net.Core;
    
    public class EqualityPredicate<T> : Predicate
    {
        private IValueSource<T> valueSource;
        
        private T v;

        public EqualityPredicate(IValueSource<T> valueSource, T v)
        {
            this.valueSource = valueSource;
            this.v = v;
        }

        public override bool Value
        {
            get
            {
                var v1 = new Maybe<T>(this.valueSource.Value);
                var v2 = new Maybe<T>(v);
                return v1.Equals(v2);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.valueSource = null;
                v = default(T);
            }

            base.Dispose(disposing);
        }
    }
}
