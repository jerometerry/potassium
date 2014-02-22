namespace Sodium
{
    public class EventSink<TA> : Event<TA>
    {
        public void Send(TA a)
        {
            Transaction.Run(new Handler<Transaction>(t => Send(t, a)));
        }
    }
}