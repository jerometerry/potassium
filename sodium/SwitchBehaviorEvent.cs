namespace Sodium
{
    internal class SwitchBehaviorEvent<TA> : Event<TA>
    {
        public SwitchBehaviorEvent(Behavior<Behavior<TA>> bba)
        {
            var callback = new SwitchBehaviorCallback<TA>(this);
            var l1 = bba.Value().Listen(callback, this.Rank);
            this.RegisterListener(l1);
        }
    }
}
