using CohesiveRP.Common.WebApi;
using Microsoft.AspNetCore.Mvc;

namespace CohesiveRP.Core.WebApi.RequestDtos.Personas
{
    public class DeleteLorebookRequestDto : IWebApiRequestDto
    {
        [FromRoute]
        public string LorebookId { get; set; }
    }
}
