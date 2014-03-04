namespace Sodium
{
    internal class LastFiringEvent<T> : CoalesceEvent<T>
    {
        public LastFiringEvent(Event<T> source, Scheduler scheduler)
            : base(source, (a, b) => b, scheduler)
        {
        }
    }
}
