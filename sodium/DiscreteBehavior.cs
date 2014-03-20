namespace Sodium
{
    public abstract class DiscreteBehavior<T> : DisposableObject, IBehavior<T>
    {
        public abstract T Value { get; }
    }
}
