namespace Sodium
{
    internal class ValuesListFiringEvent<T> : LastFiringEvent<T>
    {
        public ValuesListFiringEvent(IValue<T> behavior, Transaction transaction)
            : base(new ValueEventSink<T>(behavior, transaction), transaction)
        {
            this.RegisterFinalizer(this.Source);
        }
    }
}