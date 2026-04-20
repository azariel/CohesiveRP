using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.WebApi.RequestDtos.InteractiveUserInputQueries.BusinessObjects;
using Microsoft.AspNetCore.Mvc;

namespace CohesiveRP.Core.WebApi.RequestDtos.InteractiveUserInputQueries
{
    public class PutInteractiveUserInputQueryRequestDto : IWebApiRequestDto
    {
        [FromRoute]
        public string QueryId { get; set; }

        [FromBody]
        public PutInteractiveUserInputQueryRequest Query { get; set; }
    }
}
