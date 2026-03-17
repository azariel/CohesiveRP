using CohesiveRP.Common.WebApi;
using Microsoft.AspNetCore.Mvc;

namespace CohesiveRP.Core.WebApi.RequestDtos.Personas
{
    public class DeletePersonaRequestDto : IWebApiRequestDto
    {
        [FromRoute]
        public string PersonaId { get; set; }
    }
}
