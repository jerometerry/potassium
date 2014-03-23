using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Potassium.Dispatchers
{
    public class CurrentThreadDispatcher : IDispatcher
    {
        public CurrentThreadDispatcher()
        {
        }

        public void Dispatch(Action action)
        {
            action();
        }
    }
}
