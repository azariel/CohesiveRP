using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.WebApi.RequestDtos.Personas.BusinessObjects;
using Microsoft.AspNetCore.Mvc;

namespace CohesiveRP.Core.WebApi.RequestDtos.Personas
{
    public class UpdatePersonaRequestDto : IWebApiRequestDto
    {
        [FromBody]
        public PersonaRequest Persona { get; set; }

        [FromRoute]
        public string PersonaId { get; set; }
    }
}
