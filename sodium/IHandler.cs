namespace Sodium
{
    public interface IHandler<in TA>
    {
        void Run(TA a);
    }
}