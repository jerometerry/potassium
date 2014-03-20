namespace Sodium
{
    using System;

    public class MappedBehavior<TA, TB> : ContinuousBehavior<TB>
    {
        private IBehavior<TA> source;
        private Func<TA, TB> map;

        public MappedBehavior(IBehavior<TA> source, Func<TA, TB> map)
        {
            this.source = source;
            this.map = map;
        }

        public override TB Value
        {
            get
            {
                return map(source.Value);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.source = null;
                this.map = null;
            }

            base.Dispose(disposing);
        }
    }
}
