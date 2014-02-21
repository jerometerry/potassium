namespace sodium
{
    internal class DualListener : ListenerBase
    {
        private readonly IListener _one;
        private readonly IListener _two;

        public DualListener(IListener one, IListener two)
        {
            this._one = one;
            this._two = two;
        }

        public override void unlisten()
        {
            this._one.unlisten();
            this._two.unlisten();
        }
    }
}