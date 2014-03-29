namespace Potassium.Internal
{
    using Potassium.Core;

    internal class HoldBehavior<T> : Behavior<T>
    {
        public HoldBehavior(Observable<T> source, T initValue, Transaction t)
        {
            this.Observe(new LastFiringEvent<T>(source, t), initValue);
        }
    }
}
