namespace Sodium
{
    using System;

    public class ApplyBehavior<TA, TB> : DisposableObject, IBehavior<TB>
    {
        private IBehavior<TA> source;
        private IBehavior<Func<TA, TB>> bf;

        public ApplyBehavior(IBehavior<TA> source, IBehavior<Func<TA, TB>> bf)
        {
            this.source = source;
            this.bf = bf;
        }

        public TB Value
        {
            get
            {
                var map = bf.Value;
                var a = source.Value;
                var b = map(a);
                return b;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.source = null;
                this.bf = null;
            }

            base.Dispose(disposing);
        }
    }
}
