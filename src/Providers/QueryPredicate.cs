namespace Potassium.Providers
{
    using System;

    /// <summary>
    /// QueryPredicate is a Predicate who's value is computed by executing a predicate function.
    /// </summary>
    public class QueryPredicate : Predicate
    {
        private Func<bool> predicate;

        /// <summary>
        /// Constructs a new QueryPredicate from the predicate function
        /// </summary>
        /// <param name="predicate">Predicate function to execute when the Value is requested</param>
        public QueryPredicate(Func<bool> predicate)
        {
            this.predicate = predicate;
        }

        /// <summary>
        /// Evaluates the value of the Provider
        /// </summary>
        public override bool Value
        {
            get
            {
                return predicate();
            }
        }

        /// <summary>
        /// Clean up all resources used by the current SodiumObject
        /// </summary>
        /// <param name="disposing">Whether to dispose managed resources
        /// </param>
        protected override void Dispose(bool disposing)
        {
            this.predicate = null;

            base.Dispose(disposing);
        }
    }
}
