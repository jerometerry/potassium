namespace Sodium
{
    public abstract class BehaviorOfBehaviors<T> : DisposableObject, IBehavior<IBehavior<T>>
    {
        public abstract IBehavior<T> Value { get; }
    }
}
