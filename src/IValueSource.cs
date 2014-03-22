namespace JT.Rx.Net
{
    public interface IValueSource<out T>
    {
        T Value
        {
            get;
        }
    }
}
