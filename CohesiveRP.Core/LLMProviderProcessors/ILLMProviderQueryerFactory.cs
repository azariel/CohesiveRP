using CohesiveRP.Storage.DataAccessLayer.AIQueries;

namespace CohesiveRP.Core.LLMProviderManager
{
    public interface ILLMProviderQueryerFactory
    {
        ILLMQueryProcessor Generate(BackgroundQueryDbModel queryModel);
    }
}
