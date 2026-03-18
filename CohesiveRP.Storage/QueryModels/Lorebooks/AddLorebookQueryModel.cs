using CohesiveRP.Storage.DataAccessLayer.Lorebooks.BusinessObjects;

namespace CohesiveRP.Storage.QueryModels.Lorebooks
{
    public class AddLorebookQueryModel
    {
        public string Name { get; set; }
        public List<LorebookEntry> Entries { get; set; }
    }
}
