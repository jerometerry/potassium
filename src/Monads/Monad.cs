namespace JT.Rx.Net.Monads
{
    using JT.Rx.Net.Core;

    public abstract class Monad<T> : Disposable, IValueSource<T>
    {
        /// <summary>
        /// Sample the Behaviors current value
        /// </summary>
        public abstract T Value { get; }
    }
}
