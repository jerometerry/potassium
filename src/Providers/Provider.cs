namespace Potassium.Providers
{
    using Potassium.Core;

    /// <summary>
    /// A Monad lazily evaluates a value, and has monadic operations that allow Monads to be chained together
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>http://mikehadlow.blogspot.co.uk/2011/01/monads-in-c1-introduction.html</remarks>
    public abstract class Provider<T> : Disposable, IProvider<T>
    {
        /// <summary>
        /// Evaluates the value of the Monad
        /// </summary>
        public abstract T Value { get; }
    }
}
