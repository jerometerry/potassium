namespace Sodium
{
    internal class LastFiringEvent<T> : CoalesceEvent<T>
    {
        public LastFiringEvent(IObservable<T> source, Transaction transaction)
            : base(source, (a, b) => b, transaction)
        {
        }
    }
}
