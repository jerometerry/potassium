namespace Potassium.Providers
{
    using Potassium.Core;
    
    /// <summary>
    /// EqualityPredicate is a Predicate that (lazily) determines if the value of an IProvider is equal to a given constant value
    /// </summary>
    /// <typeparam name="T">The underlying type of the Predicate</typeparam>
    /// <remarks>EqualityPredicate is lazy in that the equality check doesn't happen until the Value (bool) is requestd.</remarks>
    public class EqualityPredicate<T> : Predicate
    {
        private IProvider<T> provider;
        
        private T v;

        public EqualityPredicate(IProvider<T> provider, T v)
        {
            this.provider = provider;
            this.v = v;
        }

        public override bool Value
        {
            get
            {
                var v1 = new Maybe<T>(this.provider.Value);
                var v2 = new Maybe<T>(v);
                return v1.Equals(v2);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.provider = null;
                v = default(T);
            }

            base.Dispose(disposing);
        }
    }
}
