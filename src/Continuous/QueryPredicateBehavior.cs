namespace JT.Rx.Net.Continuous
{
    using System;

    /// <summary>
    /// QueryPredicateBehavior is a continuous Behavior of boolean values (i.e. a predicate)
    /// who's value is computed by evaluating the supplied predicate function.
    /// </summary>
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
