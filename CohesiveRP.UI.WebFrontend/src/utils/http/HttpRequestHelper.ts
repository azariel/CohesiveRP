import type { ServerApiExceptionResponseDto } from "../../ResponsesDto/Exceptions/ServerApiExceptionResponseDto";
import type { ServerApiResponseDto } from "../../ResponsesDto/ServerApiResponseDto";
import { extractFromBody } from "./response/HttpResponseUtils";

// Avoid CORS batshit mistmatches TODO: doesn't work..?
const ServerApiUrlPrefix = `http://${window.location.hostname}:7081/`;

function isAbortError(err: unknown): boolean {
  return err instanceof DOMException && err.name === "AbortError";
}

export async function getFromServerApiAsync<T extends ServerApiResponseDto>(url:string, signal?: AbortSignal): Promise<T | null> {
    try {
      const response = await fetch(`${ServerApiUrlPrefix}${url}`, {
        method: "GET",
        headers: { "Content-Type": "application/json" },
        signal
      });

      // TODO: generalize this, it's the same as POST
      let result:T | null = await extractFromBody<T>(response.body);
      if (!response.ok) {
        // TODO: notification to user
        let exception: ServerApiExceptionResponseDto | null = result as ServerApiExceptionResponseDto | null;
        console.error(`GET HttpRequest to CohesiveRP backend Webapi failed. Response Error Code=[${exception?.code}], Message=[${exception?.message}].`);
        return result;
      }
      
      console.log(`GET [${url}] HttpRequest to CohesiveRP backend Webapi was successful.`);
      console.log(`${JSON.stringify(result)}`);
      return result;
    } catch (err) {
      if (isAbortError(err)) return null;
      console.error(`Unhandled error on getFromServerApiAsync function in HttpRequestHelper. err=[${err}].`);
    }

    return null;
}

export async function getBlobFromServerApiAsync(url: string, signal?: AbortSignal): Promise<Blob | null> {
  try {
    const response = await fetch(`${ServerApiUrlPrefix}${url}`, {
      method: "GET",
      signal
    });

    if (!response.ok) {
      console.error(`GET (blob) HttpRequest to CohesiveRP backend Webapi failed. Status=[${response.status}].`);
      return null;
    }

    console.log(`GET (blob) [${url}] HttpRequest to CohesiveRP backend Webapi was successful.`);
    return await response.blob();
  } catch (err) {
    if (isAbortError(err)) return null;
    console.error(`Unhandled error on getBlobFromServerApiAsync function in HttpRequestHelper. err=[${err}].`);
  }

  return null;
}

export async function postToServerApiAsync<T extends ServerApiResponseDto>(url:string, payload: any, signal?: AbortSignal): Promise<T | null> {
    try {
      const isFormData = payload instanceof FormData;
      const response = await fetch(`${ServerApiUrlPrefix}${url}`, {
        method: "POST",
        headers: isFormData ? {} : { "Content-Type": "application/json" },
        body: isFormData ? payload : JSON.stringify(payload),
        signal
      });

      let result:T | null = await extractFromBody<T>(response.body);
      if (!response.ok) {
        // TODO: notification to user
        let exception: ServerApiExceptionResponseDto | null = result as ServerApiExceptionResponseDto | null;
        console.error(`POST HttpRequest to CohesiveRP backend Webapi failed. Response Error Code=[${exception?.code}], Message=[${exception?.message}].`);
        return result;
      }
      
      console.log(`POST [${url}] HttpRequest to CohesiveRP backend Webapi was successful.`);
      console.log(`${JSON.stringify(result)}`);
      return result;
    } catch (err) {
      if (isAbortError(err)) return null;
      console.error(`Unhandled error on postToServerApiAsync function in HttpRequestHelper. err=[${err}].`);
    }

    return null;
}

export async function putToServerApiAsync<T extends ServerApiResponseDto>(url:string, payload: any, signal?: AbortSignal): Promise<T | null> {
    try {
      const response = await fetch(`${ServerApiUrlPrefix}${url}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload),
        signal
      });

      let result:T | null = await extractFromBody<T>(response.body);
      if (!response.ok) {
        // TODO: notification to user
        let exception: ServerApiExceptionResponseDto | null = result as ServerApiExceptionResponseDto | null;
        console.error(`PUT HttpRequest to CohesiveRP backend Webapi failed. Response Error Code=[${exception?.code}], Message=[${exception?.message}].`);
        return result;
      }
      
      console.log(`PUT [${url}] HttpRequest to CohesiveRP backend Webapi was successful.`);
      console.log(`${JSON.stringify(result)}`);
      return result;
    } catch (err) {
      if (isAbortError(err)) return null;
      console.error(`Unhandled error on putToServerApiAsync function in HttpRequestHelper. err=[${err}].`);
    }

    return null;
}

export async function deleteFromServerApiAsync<T extends ServerApiResponseDto>(url:string, signal?: AbortSignal): Promise<T | null> {
    try {
      const response = await fetch(`${ServerApiUrlPrefix}${url}`, {
        method: "DELETE",
        signal
      });

      let result:T | null = await extractFromBody<T>(response.body);
      if (!response.ok) {
        // TODO: notification to user
        let exception: ServerApiExceptionResponseDto | null = result as ServerApiExceptionResponseDto | null;
        console.error(`DELETE HttpRequest to CohesiveRP backend Webapi failed. Response Error Code=[${exception?.code}], Message=[${exception?.message}].`);
        return result;
      }
      
      console.log(`DELETE [${url}] HttpRequest to CohesiveRP backend Webapi was successful.`);
      console.log(`${JSON.stringify(result)}`);
      return result;
    } catch (err) {
      if (isAbortError(err)) return null;
      console.error(`Unhandled error on deleteToServerApiAsync function in HttpRequestHelper. err=[${err}].`);
    }

    return null;
}
