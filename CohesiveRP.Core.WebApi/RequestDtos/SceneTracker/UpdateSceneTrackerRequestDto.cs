using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.WebApi.RequestDtos.Personas.BusinessObjects;
using Microsoft.AspNetCore.Mvc;

namespace CohesiveRP.Core.WebApi.RequestDtos.Personas
{
    public class UpdateSceneTrackerRequestDto : IWebApiRequestDto
    {
        [FromBody]
        public SceneTrackerRequest SceneTracker { get; set; }

        [FromRoute]
        public string ChatId { get; set; }
    }
}
