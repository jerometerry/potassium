﻿namespace Potassium.Internal
{
    using Potassium.Core;

    internal class LastFiringEvent<T> : CoalesceEvent<T>
    {
        public LastFiringEvent(Observable<T> source, Transaction transaction)
            : base(source, (a, b) => b, transaction)
        {
        }
    }
}
