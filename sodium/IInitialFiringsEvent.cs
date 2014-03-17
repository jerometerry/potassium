using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sodium
{
    public interface IInitialFiringsEvent<T> : IEvent<T>
    {
        T[] InitialFirings();
    }
}
