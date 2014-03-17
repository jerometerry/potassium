namespace Sodium
{
    internal class LastFiring<T> : Coalesce<T>
    {
        public LastFiring(IEvent<T> source, Transaction transaction)
            : base(source, (a, b) => b, transaction)
        {
        }
    }
}
