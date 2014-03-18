namespace Sodium
{
    internal sealed class NotNullFilterEvent<T> : FilterEvent<T>
    {
        public NotNullFilterEvent(IObservable<T> source)
            : base(source, a => a != null)
        {
        }
    }
}
