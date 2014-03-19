namespace Sodium
{
    /// <summary>
    /// BehaviorLoop is a Behavior where the underlying Event is an EventLoop.
    /// </summary>
    /// <typeparam name="T">The type of values published through the Behavior</typeparam>
    public class BehaviorLoop<T> : Behavior<T>
    {
        /// <summary>
        /// Constructs a new BehaviorLoop
        /// </summary>
        /// <param name="initValue">The initial value of the Behavior</param>
        public BehaviorLoop(T initValue)
            : base(new EventLoop<T>(), initValue)
        {
            this.Register(this.Source);
        }

        /// <summary>
        ///  Firings on the given Event will be forwarded to the current Event
        /// </summary>
        /// <param name="source">Event who's publishings will be looped to the current Event</param>
        public void Loop(IObservable<T> source)
        {
            var loop = (EventLoop<T>)this.Source;
            loop.Loop(source);
        }
    }
}
