namespace Sodium
{
    using System;

    internal class CollectEvent<T, TB, TS> : EventLoop<TB>
    {
        public CollectEvent(IObservable<T> source, TS initState, Func<T, TS, Tuple<TB, TS>> snapshot)
        {
            var es = new EventLoop<TS>();
            var s = es.Hold(initState);
            var ebs = source.Snapshot(s, snapshot);
            var eb = ebs.Map(bs => bs.Item1);
            var evt = ebs.Map(bs => bs.Item2);
            es.Loop(evt);
            this.Loop(eb);

            this.RegisterFinalizer(es);
            this.RegisterFinalizer(s);
            this.RegisterFinalizer(ebs);
            this.RegisterFinalizer(evt);
            this.RegisterFinalizer(eb);
        }
    }
}