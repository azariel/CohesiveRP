using Microsoft.AspNetCore.Mvc;

namespace CohesiveRP.Core.WebApi.RequestDtos.IllustrationQueries
{
    public class UpdateIllustrationQueryRequestDto : AddIllustrationQueryRequestDto
    {
        [FromRoute]
        public string QueryId { get; set; }
    }
}
