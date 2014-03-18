namespace Sodium
{
    internal class SubscribeFireLastValueEvent<T> : LastFiring<T>
    {
        public SubscribeFireLastValueEvent(IValue<T> valueStream, Transaction transaction)
            : base(new SubscribeFireValueEvent<T>(valueStream, transaction), transaction)
        {
            this.RegisterFinalizer(this.Source);
        }
    }
}