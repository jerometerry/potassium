namespace Sodium
{
    using System;

    public class QueryBehavior<T> : ContinuousBehavior<T>
    {
        private Func<T> query;

        public QueryBehavior(Func<T> query)
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
