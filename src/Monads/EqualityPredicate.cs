namespace JT.Rx.Net.Monads
{
    using JT.Rx.Net.Core;
    
    /// <summary>
    /// EqualityPredicate is a Predicate that (lazily) determines if the value of an IValueSource is equal to a given constant value
    /// </summary>
    /// <typeparam name="T">The underlying type of the Predicate</typeparam>
    /// <remarks>EqualityPredicate is lazy in that the equality check doesn't happen until the Value (bool) is requestd.</remarks>
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
