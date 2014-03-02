namespace Sodium
{
    public sealed class BehaviorLoop<T> : Behavior<T>
    {
        public BehaviorLoop()
            : base(new EventLoop<T>(), default(T))
        {
            this.RegisterFinalizer(this.Source);
        }

        /// <summary>
        /// Firings on b will be forwarded to the current Behavior
        /// </summary>
        /// <param name="b">Behavior who's firings will be looped to the current Behavior</param>
        /// <returns>The current Behavior</returns>
        /// <remarks>Loop can only be called once on a Behavior. If Loop is called multiple times,
        /// an ApplicationException will be raised.</remarks>
        public Behavior<T> Loop(Behavior<T> b)
        {
            var loop = (EventLoop<T>)Source;
            loop.Loop(b.Updates());
            return this;
        }
    }
}
