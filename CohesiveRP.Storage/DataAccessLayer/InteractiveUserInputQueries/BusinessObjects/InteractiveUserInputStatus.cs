namespace CohesiveRP.Storage.DataAccessLayer.InteractiveUserInputQueries.BusinessObjects
{
    public enum InteractiveUserInputStatus
    {
        WaitingUserInput = 0,
        Processing = 1,
        Pending = 2,
        WaitingOnBackgroundTask = 3,
        Completed = 4,
        Error = 5,
    }
}
