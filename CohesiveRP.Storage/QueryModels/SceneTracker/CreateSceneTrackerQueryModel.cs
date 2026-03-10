namespace CohesiveRP.Storage.QueryModels.SceneTracker
{
    public class CreateSceneTrackerQueryModel
    {
        public string ChatId { get; set; }
        public DateTime? CreatedAtUtc { get; set; }
        public string LinkMessageId { get; set; }
        public string Content { get; set; }
    }
}
