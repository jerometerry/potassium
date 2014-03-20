namespace Sodium
{
    public abstract class BehaviorOfEvents<T> : DisposableObject, IBehavior<Event<T>>
    {
        public abstract Event<T> Value { get; }
    }
}
