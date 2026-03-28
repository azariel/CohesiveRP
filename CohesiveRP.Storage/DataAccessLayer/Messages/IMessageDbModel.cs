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
        string AvatarFilePath { get; }
    }
}
