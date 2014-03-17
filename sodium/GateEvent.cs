namespace Sodium
{
    using System;

    internal class GateEvent<T> : EventLoop<T>
    {
        public GateEvent(IObservable<T> source, IValue<bool> predicate)
        {
            Func<T, bool, Maybe<T>> snapshot = (a, p) => p ? new Maybe<T>(a) : null;
            var sn = source.Snapshot(predicate, snapshot);
            var filter = sn.FilterNotNull();
            var map = filter.Map(a => a.Value());
            this.Loop(map);

            this.RegisterFinalizer(filter);
            this.RegisterFinalizer(sn);
            this.RegisterFinalizer(map);
        }
    }
}