namespace Sodium
{
    internal class LastFiring<T> : Coalesce<T>
    {
        public LastFiring(IObservable<T> source, Transaction transaction)
            : base(source, (a, b) => b, transaction)
        {
        }
    }
}
