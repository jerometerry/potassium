namespace Sodium
{
    internal class SubscribeFireLastValueEvent<T> : LastFiringEvent<T>
    {
        public SubscribeFireLastValueEvent(IValue<T> valueStream, Transaction transaction)
            : base(new SubscribeFireValueEvent<T>(valueStream, transaction), transaction)
        {
            this.Register(this.Source);
        }
    }
}