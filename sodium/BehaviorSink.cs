namespace sodium
{
    public class BehaviorSink<TA> : Behavior<TA>
    {
        public BehaviorSink(TA initVal)
            : base(new EventSink<TA>(), initVal)
        {
        }

        public void Send(TA a)
        {
            ((EventSink<TA>)Event).Send(a);
        }
    }
}