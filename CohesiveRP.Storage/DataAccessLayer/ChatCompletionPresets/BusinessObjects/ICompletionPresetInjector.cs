using CohesiveRP.Storage.DataAccessLayer.AIQueries;

namespace CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects
{
    internal interface ICompletionPresetInjector
    {
        internal ChatCompletionPresetsDbModel InjectPreset() => throw new NotImplementedException();
    }
}
