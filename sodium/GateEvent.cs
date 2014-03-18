namespace Sodium
{
    using System;

    internal class GateEvent<T> : EventLoop<T>
    {
        public GateEvent(IEvent<T> source, IValue<bool> predicate)
        {
            Func<T, bool, Maybe<T>> snapshot = (a, p) => p ? new Maybe<T>(a) : null;
            var sn = source.Snapshot(predicate, snapshot);
            var filter = sn.FilterNotNull();
            var map = filter.Map(a => a.Value());
            this.Loop(map);

            this.Register(filter);
            this.Register(sn);
            this.Register(map);
        }
    }
}