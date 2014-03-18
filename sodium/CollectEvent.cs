namespace Sodium
{
    using System;

    internal class CollectEvent<T, TB, TS> : EventLoop<TB>
    {
        public CollectEvent(IEvent<T> source, TS initState, Func<T, TS, Tuple<TB, TS>> snapshot)
        {
            var es = new EventLoop<TS>();
            var s = es.Hold(initState);
            var ebs = source.Snapshot(s, snapshot);
            var eb = ebs.Map(bs => bs.Item1);
            var evt = ebs.Map(bs => bs.Item2);
            es.Loop(evt);
            this.Loop(eb);

            this.Register(es);
            this.Register(s);
            this.Register(ebs);
            this.Register(evt);
            this.Register(eb);
        }
    }
}