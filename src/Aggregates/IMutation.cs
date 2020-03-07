namespace MindMatrix.Aggregates
{
    public interface IMutation<Aggregate>
    {
        void Apply(Aggregate aggregate);
    }
}