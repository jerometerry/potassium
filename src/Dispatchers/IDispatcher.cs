using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Potassium.Dispatchers
{
    public interface IDispatcher
    {
        void Dispatch(Action action);
    }
}
