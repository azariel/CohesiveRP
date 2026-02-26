using System.Text.Json;

namespace CohesiveRP.Storage.DataAccessLayer.Users
{
    // TODO: Cache
    public class StorageDal : IStorageDal
    {
        protected JsonSerializerOptions jsonSerializerOptions;

        public StorageDal(JsonSerializerOptions jsonSerializerOptions)
        {
            this.jsonSerializerOptions = jsonSerializerOptions;
        }
    }
}
