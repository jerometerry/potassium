namespace Sodium
{
    public sealed class BehaviorLoop<TA> : Behavior<TA>
    {
        public BehaviorLoop()
            : base(new EventLoop<TA>(), default(TA))
        {
        }

        public void Loop(Behavior<TA> b)
        {
            ((EventLoop<TA>)Event).Loop(b.Updates());
            var v = b.Sample();
            SetValue(v);
        }
    }
}