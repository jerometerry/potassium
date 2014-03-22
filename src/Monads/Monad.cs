namespace JT.Rx.Net.Monads
{
    public abstract class Monad<T> : Disposable, IValueSource<T>
    {
        /// <summary>
        /// Sample the Behaviors current value
        /// </summary>
        public abstract T Value { get; }
    }
}
