namespace MindMatrix.Aggregates
{
    public interface IMutation<T>
    {
        void Apply(T aggregate);
    }
}