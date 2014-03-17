namespace Sodium
{
    internal class ValuesListFiring<T> : LastFiring<T>
    {
        public ValuesListFiring(IValue<T> behavior, Transaction transaction)
            : base(new ValueSink<T>(behavior, transaction), transaction)
        {
            this.RegisterFinalizer(this.Source);
        }
    }
}