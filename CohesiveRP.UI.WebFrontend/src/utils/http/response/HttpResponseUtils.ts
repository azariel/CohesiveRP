import type { ServerApiExceptionResponseDto } from "../../../ResponsesDto/Exceptions/ServerApiExceptionResponseDto";
import type { ServerApiResponseDto } from "../../../ResponsesDto/ServerApiResponseDto";

export async function extractRawStringBodyFromHttpResponseAsync(bodyStream: ReadableStream<Uint8Array> | null): Promise<string | null> {
    if (!bodyStream) {
        return null;
    }

    const decoder = new TextDecoder();// default is UTF-8
    const reader = bodyStream.getReader();
    let content = "";

    try {
        while (true) {
            const { value, done } = await reader.read();

            if (value) {
                content += decoder.decode(value, { stream: true });
            }

            if (done) {
                content += decoder.decode(); // flush
                return content;
            }
        }
    } finally {
        reader.releaseLock();
    }
}

export async function extractErrorFromHttpResponseAsync(bodyStream: ReadableStream<Uint8Array> | null): Promise<ServerApiExceptionResponseDto | null> {
    let bodyAsString : string | null = await extractRawStringBodyFromHttpResponseAsync(bodyStream);

    if (!bodyAsString) {
        return null;
    }

    return JSON.parse(bodyAsString) as ServerApiExceptionResponseDto;
}

export async function extractServerApiResponseFromBody(bodyStream: ReadableStream<Uint8Array> | null): Promise<ServerApiResponseDto | null> {
    let bodyAsString : string | null = await extractRawStringBodyFromHttpResponseAsync(bodyStream);

    if (!bodyAsString) {
        return null;
    }

    return JSON.parse(bodyAsString) as ServerApiResponseDto;
}
