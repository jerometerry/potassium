namespace Potassium.Internal
{
    using System;
    using Potassium.Core;
    using Potassium.Providers;

    internal class GateEvent<T> : EventFeed<T>
    {
        public GateEvent(Event<T> source, Func<bool> predicate)
            : this(source, new QueryPredicate(predicate))
        {
        }

        public GateEvent(Event<T> source, IProvider<bool> predicate)
        {
            Func<T, bool, Maybe<T>> snapshot = (a, p) => p ? new Maybe<T>(a) : Maybe<T>.Nothing;
            var sn = source.Snapshot(snapshot, predicate);
            var filter = sn.Where(a => a.HasValue);
            var map = filter.Map(a => a.Value);
            this.Feed(map);
            this.Register(map);
            this.Register(filter);
            this.Register(sn);
        }
    }
}
