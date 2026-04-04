using CohesiveRP.Common.BusinessObjects;

namespace CohesiveRP.Storage.DataAccessLayer.Messages
{
    public interface IMessageDbModel
    {
        string MessageId { get; set; }
        string Content { get; set; }
        MessageSourceType SourceType { get; set; }
        DateTime CreatedAtUtc { get; set; }
        bool Summarized { get; set; }
        string CharacterId { get; }
        string[] AvatarsFilePath { get; }
        DateTime? InRoleplayDateTime { get; set; }
        DateTime? StartGenerationDateTimeUtc { get; set; }
        DateTime? StartFocusedGenerationDateTimeUtc { get; set; }
        DateTime? EndFocusedGenerationDateTimeUtc { get; set; }
    }
}
