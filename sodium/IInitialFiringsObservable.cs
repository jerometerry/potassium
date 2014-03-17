using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sodium
{
    public interface IInitialFiringsObservable<T> : IObservable<T>
    {
        T[] InitialFirings();
    }
}
