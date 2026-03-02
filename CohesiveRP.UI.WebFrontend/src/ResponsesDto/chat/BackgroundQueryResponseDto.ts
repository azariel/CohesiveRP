import type { ServerApiResponseDto } from "../ServerApiResponseDto";

interface BackgroundQueryResponseDto extends ServerApiResponseDto {
chatId : string,
backgroundQueryId : string,
dependenciesTags : string[],
tags : string[],
status : string,
priority : number,
content : string
}

export type {
    BackgroundQueryResponseDto
};