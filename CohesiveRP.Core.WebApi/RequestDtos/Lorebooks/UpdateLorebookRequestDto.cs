using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.WebApi.RequestDtos.Personas.BusinessObjects;
using Microsoft.AspNetCore.Mvc;

namespace CohesiveRP.Core.WebApi.RequestDtos.Personas
{
    public class UpdateLorebookRequestDto : IWebApiRequestDto
    {
        [FromRoute]
        public string LorebookId { get; set; }

        [FromBody]
        public LorebookRequest Lorebook { get; set; }
    }
}
