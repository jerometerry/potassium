namespace JT.Rx.Net.Continuous
{
    using System;

    public class QueryPredicate : Core.Predicate
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
