namespace Sodium
{
    using System;

    internal class GateEvent<T> : EventLoop<T>
    {
        public GateEvent(IObservable<T> source, Behavior<bool> predicate)
        {
            Func<T, bool, Maybe<T>> snapshot = (a, p) => p ? new Maybe<T>(a) : null;
            var sn = Transformer.Default.Snapshot(source, predicate, snapshot);
            var filter = Transformer.Default.FilterNotNull(sn);
            var map = Transformer.Default.Map(filter, a => a.Value());
            this.Loop(map);

            this.Register(filter);
            this.Register(sn);
            this.Register(map);
        }
    }
}