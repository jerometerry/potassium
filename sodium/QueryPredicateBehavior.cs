namespace Sodium
{
    using System;

    public class QueryPredicateBehavior : PredicateBehavior
    {
        private Func<bool> predicate;

        public QueryPredicateBehavior(Func<bool> predicate)
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
