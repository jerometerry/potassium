namespace Sodium
{
    public class EventSink<TA> : Event<TA>
    {
        public void Send(TA a)
        {
            Transaction.Run(t => { Fire(t, a); return Unit.Value; });
        }
    }
}