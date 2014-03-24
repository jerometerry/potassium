﻿namespace Potassium.Internal
{
    using Potassium.Core;

    internal sealed class SuppressedSubscribeEvent<T> : EventFeed<T>
    {
        public SuppressedSubscribeEvent(Observable<T> source)
        {
            this.Feed(source);
        }

        internal override bool Republish(ISubscription<T> subscription, Transaction transaction)
        {
            return false;
        }
    }
}
