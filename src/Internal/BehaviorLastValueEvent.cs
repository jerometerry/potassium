namespace JT.Rx.Net.Internal
{
    using JT.Rx.Net.Core;

    internal class BehaviorLastValueEvent<T> : LastFiringEvent<T>
    {
        public BehaviorLastValueEvent(Behavior<T> source, Transaction transaction)
            : base(new BehaviorValueEvent<T>(source, transaction), transaction)
        {
            this.Register(this.Source);
        }
    }
}