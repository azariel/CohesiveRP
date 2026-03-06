namespace CohesiveRP.Storage.DataAccessLayer.BackgroundQueries.BusinessObjects
{
    public enum BackgroundQueryStatus
    {
        Pending = 0,
        InProgress = 1,

        ProcessedWaitingForFinalInstruction = 10,
        ProcessingFinalInstruction = 11,

        Completed = 99,

        Error = 100,
    }
}
