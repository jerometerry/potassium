namespace Sodium
{
    public class EventSink<TA> : Event<TA>
    {
        public void Send(TA a)
        {
            Transaction.Run(t => Send(t, a));
        }
    }
}