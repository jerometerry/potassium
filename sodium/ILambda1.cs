namespace Sodium
{
    public interface ILambda1<in TA, out TB>
    {
        TB Apply(TA a);
    }
}