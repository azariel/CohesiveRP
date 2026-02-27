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

export async function extractFromBody<T>(bodyStream: ReadableStream<Uint8Array> | null): Promise<T | null> {
    let bodyAsString : string | null = null;
    try {
        bodyAsString = await extractRawStringBodyFromHttpResponseAsync(bodyStream);

        if (!bodyAsString) {
            return null;
        }

        let bodyJsonResponse = JSON.parse(bodyAsString);
        return JSON.parse(bodyJsonResponse) as T | null;
    } catch(err) {
        console.error(`Couldn't convert the HttpResponse body to valid Json: [${err}]. Initial stringified body to convert: [${bodyAsString}].`);
        return null;
    }
}
