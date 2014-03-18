using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sodium
{
    /// <summary>
    /// A Behavior who's value never changes, even if fired new values.
    /// </summary>
    /// <typeparam name="T">The type of values fired through the Behavior</typeparam>
    public class ConstantBehavior<T> : Behavior<T>
    {
        /// <summary>
        /// Constructs a new ConstantBehavior
        /// </summary>
        /// <param name="initValue">The constant initial value of the Behavior</param>
        public ConstantBehavior(T initValue)
            : base(new ValueContainer<T>(initValue))
        {
        }
    }
}
