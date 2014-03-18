using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sodium
{
    public class ConstantBehavior<T> : Behavior<T>
    {
        public ConstantBehavior(T initValue)
            : base(new ValueContainer<T>(initValue))
        {
        }
    }
}
