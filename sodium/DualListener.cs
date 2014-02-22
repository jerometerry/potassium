namespace Sodium
{
    internal sealed class DualListener : ListenerBase
    {
        private readonly IListener listener1;
        private readonly IListener listener2;

        public DualListener(IListener listener1, IListener listener2)
        {
            this.listener1 = listener1;
            this.listener2 = listener2;
        }

        public override void Unlisten()
        {
            listener1.Unlisten();
            listener2.Unlisten();
        }
    }
}