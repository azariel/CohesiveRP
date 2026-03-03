import type { ServerApiResponseDto } from "../ServerApiResponseDto";

interface ExceptionResponseDto extends ServerApiResponseDto {
    message: string,
}

export type {
    ExceptionResponseDto
};