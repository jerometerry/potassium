namespace Sodium.Continuous
{
    using Sodium.Core;
    using System;

    public class Map<TA,TB> : IBehavior<Func<TA, TB>>
    {
        private Func<TA, TB> map;

        public Map(Func<TA, TB> map)
        {
            this.map = map;
        }

        public Func<TA, TB> Value
        {
            get
            {
                return this.map;
            }
        }
    }
}