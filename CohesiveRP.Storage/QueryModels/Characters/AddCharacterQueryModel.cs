namespace CohesiveRP.Storage.QueryModels.Chat
{
    public record AddCharacterQueryModel
    {
        public string Name { get; set; }
        public string Creator { get; set; }
        public string CreatorNotes { get; set; }
        public string Description { get; set; }
        public List<string> Tags { get; set; }
        public string FirstMessage { get; set; }
        public List<string> AlternateGreetings { get; set; }
    }
}
