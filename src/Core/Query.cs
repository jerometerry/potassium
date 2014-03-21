namespace JT.Rx.Net.Core
{
    using System;

    /// <summary>
    /// QueryBehavior is a continuous Behavior who's value is computed using a query method.
    /// </summary>
    /// <typeparam name="T">The type of the return value of the query function</typeparam>
    public class Query<T> : Monad<T>
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
