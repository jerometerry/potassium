namespace Potassium.Internal
{
    using Potassium.Core;

    internal sealed class SuppressedSubscribeEvent<T> : EventRepeater<T>
    {
        public SuppressedSubscribeEvent(Observable<T> source)
        {
            this.Repeat(source);
        }

        internal override bool Refire(ISubscription<T> subscription, Transaction transaction)
        {
            return false;
        }
    }
}
