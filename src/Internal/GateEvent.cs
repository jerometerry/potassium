namespace Potassium.Internal
{
    using System;
    using Potassium.Core;
    using Potassium.Providers;

    internal class GateEvent<T> : EventFeed<T>
    {
        public GateEvent(Event<T> source, Predicate predicate)
        {
            Func<T, bool, Maybe<T>> snapshot = (a, p) => p ? new Maybe<T>(a) : null;
            var sn = source.Snapshot(snapshot, predicate);
            var filter = sn.Where(a => a != null);
            var map = filter.Map(a => a.Value);
            this.Feed(map);
            this.Register(map);
            this.Register(filter);
            this.Register(sn);
        }
    }
}
