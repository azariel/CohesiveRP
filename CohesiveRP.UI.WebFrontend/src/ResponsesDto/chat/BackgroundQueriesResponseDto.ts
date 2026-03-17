import type { ServerApiResponseDto } from "../ServerApiResponseDto";
import type { BackgroundQuery } from "./BusinessObjects/BackgroundQuery";

interface BackgroundQueriesResponseDto extends ServerApiResponseDto {
    chatId : string,
    queries: BackgroundQuery[]
}

export type {
    BackgroundQueriesResponseDto
};