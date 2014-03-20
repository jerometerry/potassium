namespace Sodium
{
    public abstract class PredicateBehavior : DisposableObject, IBehavior<bool>
    {
        public abstract bool Value { get; }
    }
}
