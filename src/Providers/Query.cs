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

        public Query(Func<T> query)
        {
            this.query = query;
        }

        public override T Value
        {
            get
            {
                return query();
            }
        }
    }
}
