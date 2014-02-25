namespace Sodium
{
    /// <summary>
    /// A BehaviorSink is a Behavior that can send new values
    /// </summary>
    /// <typeparam name="TA"></typeparam>
    public sealed class BehaviorSink<TA> : Behavior<TA>
    {
        public BehaviorSink(TA initValue)
            : base(new Event<TA>(), initValue)
        {
        }

        public void Fire(TA a)
        {
            this.Event.Fire(a);
        }
    }
}