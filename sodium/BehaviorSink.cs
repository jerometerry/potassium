namespace sodium
{
    public class BehaviorSink<A> : Behavior<A>
    {
        public BehaviorSink(A initVal)
            : base(new EventSink<A>(), initVal)
        {
        }

        public void Send(A a)
        {
            ((EventSink<A>)Event).send(a);
        }
    }
}