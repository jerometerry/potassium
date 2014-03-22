namespace JT.Rx.Net
{
    public abstract class Monad<T> : Disposable, IValueSource<T>
    {
        /// <summary>
        /// Sample the Behaviors current value
        /// </summary>
        public abstract T Value { get; }
    }
}
