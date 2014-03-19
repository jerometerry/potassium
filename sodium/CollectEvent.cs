namespace Sodium
{
    using System;

    internal class CollectEvent<T, TB, TS> : EventLoop<TB>
    {
        public CollectEvent(IObservable<T> source, TS initState, Func<T, TS, Tuple<TB, TS>> snapshot)
        {
            var es = new EventLoop<TS>();
            var s = Transformer.Default.Hold(es, initState);
            var ebs = Transformer.Default.Snapshot(source, s, snapshot);
            var eb = Transformer.Default.Map(ebs, bs => bs.Item1);
            var evt = Transformer.Default.Map(ebs, bs => bs.Item2);
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