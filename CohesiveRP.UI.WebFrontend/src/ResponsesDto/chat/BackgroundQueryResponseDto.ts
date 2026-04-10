import type { ServerApiResponseDto } from "../ServerApiResponseDto";

interface BackgroundQueryResponseDto extends ServerApiResponseDto {
    chatId : string,
    linkedId : string,
    backgroundQueryId : string,
    dependenciesTags : string[],
    tags : string[],
    status : string,
    priority : number,
    content : string,
    startFocusedGenerationDateTimeUtc: string,
    endFocusedGenerationDateTimeUtc: string,
    createdAtUtc : string
}

export type {
    BackgroundQueryResponseDto
};