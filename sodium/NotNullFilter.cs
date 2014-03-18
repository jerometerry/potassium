namespace Sodium
{
    internal sealed class NotNullFilter<T> : Filter<T>
    {
        public NotNullFilter(IObservable<T> source)
            : base(source, a => a != null)
        {
        }
    }
}
