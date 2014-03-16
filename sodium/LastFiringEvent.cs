namespace Sodium
{
    internal sealed class LastFiringEvent<T> : CoalesceEvent<T>
    {
        public LastFiringEvent(Event<T> source, Transaction transaction)
            : base(source, (a, b) => b, transaction)
        {
        }
    }
}
