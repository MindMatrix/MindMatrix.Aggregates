namespace MindMatrix.Aggregates
{
    using Newtonsoft.Json;

    public interface IMutationSerializer<Aggregate>
    {
        IMutation<Aggregate> Deserialize(MutationType mutationType, string data);

        string Serialize(MutationType mutationType, IMutation<Aggregate> mutation);
    }

    public class NewtonsoftMutationSerializer<Aggregate> : IMutationSerializer<Aggregate>
    {
        //private readonly JsonSerializerSettings _jsonSettings;

        // public NewtonsoftMutationSerializer(JsonSerializerSettings jsonSettings)
        // {
        //     _jsonSettings = jsonSettings;
        // }

        public IMutation<Aggregate> Deserialize(MutationType mutationType, string mutationData) => JsonConvert.DeserializeObject(mutationData, mutationType.Type) as IMutation<Aggregate>;

        public string Serialize(MutationType mutationType, IMutation<Aggregate> mutation) => JsonConvert.SerializeObject(mutation, mutationType.Type, null);
    }

}