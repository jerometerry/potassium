namespace Potassium.Providers
{
    using System;

    /// <summary>
    /// QueryPredicate is a Predicate who's value is computed by executing a predicate function.
    /// </summary>
    public class QueryPredicate : Predicate
    {
        private Func<bool> predicate;

        public QueryPredicate(Func<bool> predicate)
        {
            this.predicate = predicate;
        }

        public override bool Value
        {
            get
            {
                return predicate();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.predicate = null;
            }

            base.Dispose(disposing);
        }
    }
}
