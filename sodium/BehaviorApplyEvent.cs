using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sodium
{
    internal class BehaviorApplyEvent<TA,TB> : Event<TB>
    {
        Behavior<TB> behavior;
        IListener listner1;
        IListener listner2;

        public BehaviorApplyEvent(Behavior<Func<TA, TB>> bf, Behavior<TA> ba)
        {
            var h = new BehaviorApplyHandler<TA, TB>(this, bf, ba);
            var functionChanged = new Callback<Func<TA, TB>>((t, f) => h.Run(t));
            var valueChanged = new Callback<TA>((t, a) => h.Run(t));
            listner1 = bf.Updates().Listen(functionChanged, this.Rank);
            listner2 = ba.Updates().Listen(valueChanged, this.Rank);
            var map = bf.Sample();
            var valA = ba.Sample();
            var valB = map(valA);
            behavior = this.Hold(valB);
        }

        public Behavior<TB> Behavior
        {
            get { return behavior; }
        }
    }
}
