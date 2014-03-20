namespace Sodium
{
    public abstract class ContinuousBehavior<T> : DisposableObject, IBehavior<T>
    {
        public abstract T Value { get; }
    }
}
