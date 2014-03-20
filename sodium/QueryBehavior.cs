namespace Sodium
{
    using System;

    public class QueryBehavior<T> : DisposableObject, IBehavior<T>
    {
        private Func<T> query;

        public QueryBehavior(Func<T> query)
        {
            this.query = query;
        }

        public T Value
        {
            get
            {
                return query();
            }
        }
    }
}
