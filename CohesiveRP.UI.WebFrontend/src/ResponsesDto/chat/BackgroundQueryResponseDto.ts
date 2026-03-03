import type { ServerApiResponseDto } from "../ServerApiResponseDto";

interface BackgroundQueryResponseDto extends ServerApiResponseDto {
    chatId : string,
    linkedMessageId : string,
    backgroundQueryId : string,
    dependenciesTags : string[],
    tags : string[],
    status : string,
    priority : number,
    content : string
    createdAtUtc : string
}

export type {
    BackgroundQueryResponseDto
};