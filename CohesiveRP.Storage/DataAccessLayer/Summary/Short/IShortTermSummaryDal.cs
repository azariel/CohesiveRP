using CohesiveRP.Storage.DataAccessLayer.Messages;
using CohesiveRP.Storage.QueryModels.Message;

namespace CohesiveRP.Storage.DataAccessLayer.Summary.Short
{
    public interface IShortTermSummaryDal
    {
        Task<ISummaryDbModel> AddShortTermSummaryAsync(CreateSummaryQueryModel queryModel);
    }
}
