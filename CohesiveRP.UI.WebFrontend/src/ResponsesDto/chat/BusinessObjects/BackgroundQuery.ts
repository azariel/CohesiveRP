interface BackgroundQuery {
    priority : number,
    content : string
    createdAtUtc : string,
    linkedId : string,
    backgroundQueryId : string,
    dependenciesTags : string[],
    tags : string[]
}

export type {
    BackgroundQuery
};