namespace Sodium
{
    /// <summary>
    /// A BehaviorLoop listens for updates from another Behavior, and fires them to listeners of the current behavior
    /// </summary>
    /// <typeparam name="T">The type of the value fired through the event</typeparam>
    public sealed class BehaviorLoop<T> : Behavior<T>, ILoop<T>
    {
        /// <summary>
        /// Default constructor for a BehaviorLoop
        /// </summary>
        public BehaviorLoop()
            : base(new EventLoop<T>(), default(T))
        {
            this.RegisterFinalizer(this.Source);
        }

        /// <summary>
        /// Firings on b will be forwarded to the current Behavior
        /// </summary>
        /// <param name="observable">Observable who's firings will be looped to the current Behavior</param>
        /// <remarks>Loop can only be called once on a Behavior. If Loop is called multiple times,
        /// an ApplicationException will be raised.</remarks>
        public void Loop(IObservable<T> observable)
        {
            var loop = (ILoop<T>)Source;
            loop.Loop(observable);
        }
    }
}
