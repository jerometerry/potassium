namespace Sodium
{
    internal class BehaviorLastValueEvent<T> : LastFiringEvent<T>
    {
        public BehaviorLastValueEvent(DiscreteBehavior<T> source, Transaction transaction)
            : base(new BehaviorValueEvent<T>(source, transaction), transaction)
        {
            this.Register(this.Source);
        }
    }
}