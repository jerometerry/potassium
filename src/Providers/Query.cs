namespace Potassium.Providers
{
    using System;

    /// <summary>
    /// Query is a Monad who's value is computed by evaluating a query function.
    /// </summary>
    /// <typeparam name="T">The type of the return value of the query function</typeparam>
    public class Query<T> : Provider<T>
    {
        private Func<T> query;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        public Query(Func<T> query)
        {
            this.query = query;
        }

        /// <summary>
        /// Evaluates the value of the Provider
        /// </summary>
        public override T Value
        {
            get
            {
                return query();
            }
        }

        /// <summary>
        /// Clean up all resources used by the current SodiumObject
        /// </summary>
        /// <param name="disposing">Whether to dispose managed resources
        /// </param>
        protected override void Dispose(bool disposing)
        {
            query = null;

            base.Dispose(disposing);
        }
    }
}
