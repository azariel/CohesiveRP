type BackgroundQueryStatus =
    | "Pending"
    | "InProgress"
    | "ProcessedWaitingForFinalInstruction"
    | "ProcessingFinalInstruction"
    | "Completed"
    | "Error";

interface BackgroundQuery {
    priority : number,
    content : string
    createdAtUtc : string,// Note: not matching backend model??
    linkedId : string,
    backgroundQueryId : string,
    dependenciesTags : string[],
    tags : string[],
    status : BackgroundQueryStatus,
    startFocusedGenerationDateTimeUtc : string,
    endFocusedGenerationDateTimeUtc : string,
}

export type {
    BackgroundQuery,
    BackgroundQueryStatus
};