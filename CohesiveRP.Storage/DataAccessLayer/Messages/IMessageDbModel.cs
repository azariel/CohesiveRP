using CohesiveRP.Common.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.Messages.BusinessObjects;

namespace CohesiveRP.Storage.DataAccessLayer.Messages
{
    public interface IMessageDbModel
    {
        string MessageId { get; set; }
        string Content { get; set; }
        string ThinkingContent { get; set; }
        MessageSourceType SourceType { get; set; }
        DateTime CreatedAtUtc { get; set; }
        bool Summarized { get; set; }
        string CharacterId { get; }
        public CharacterAvatarDefinition[] CharacterAvatars { get; set; }
        DateTime? InRoleplayDateTime { get; set; }
        DateTime? StartGenerationDateTimeUtc { get; set; }
        DateTime? StartFocusedGenerationDateTimeUtc { get; set; }
        DateTime? EndFocusedGenerationDateTimeUtc { get; set; }
    }
}
