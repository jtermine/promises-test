using Termine.Promises.Generics;

namespace TestConsumePromise.Collections
{
    public class NewCollectionRequest: GenericRequest
    {
        public string CollectionName { get; set; }
        public int Visibility { get; set; }
    }
}