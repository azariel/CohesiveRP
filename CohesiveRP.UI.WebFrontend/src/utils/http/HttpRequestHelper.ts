import type { ServerApiExceptionResponseDto } from "../../ResponsesDto/Exceptions/ServerApiExceptionResponseDto";
import type { ServerApiResponseDto } from "../../ResponsesDto/ServerApiResponseDto";
import { extractFromBody } from "./response/HttpResponseUtils";

// Avoid CORS batshit mistmatches TODO: doesn't work..?
const ServerApiUrlPrefix = `http://${window.location.hostname}:7081/`;

export async function getFromServerApiAsync<T extends ServerApiResponseDto>(url:string): Promise<T | null> {
    try {
      const response = await fetch(`${ServerApiUrlPrefix}${url}`, {
        method: "GET",
        headers: { "Content-Type": "application/json" }
      });

      // TODO: generalize this, it's the same as POST
      let result:T | null = await extractFromBody<T>(response.body);
      if (!response.ok) {
        // TODO: notification to user
        let exception: ServerApiExceptionResponseDto | null = result as ServerApiExceptionResponseDto | null;
        console.error(`GET HttpRequest to CohesiveRP backend Webapi failed. Response Error Code=[${exception?.code}], Message=[${exception?.message}].`);
        return result;
      }
      
      console.log("GET HttpRequest to CohesiveRP backend Webapi was successful. Raw response:", JSON.stringify(result));
      return result;
    } catch (err) {
      console.error(`Unhandled error on getFromServerApiAsync function in HttpRequestHelper. err=[${err}].`);
    }

    return null;
}

export async function postToServerApiAsync<T extends ServerApiResponseDto>(url:string, payload: any): Promise<T | null> {
    try {
      const response = await fetch(`${ServerApiUrlPrefix}${url}`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload),
      });

      let result:T | null = await extractFromBody<T>(response.body);
      if (!response.ok) {
        // TODO: notification to user
        let exception: ServerApiExceptionResponseDto | null = result as ServerApiExceptionResponseDto | null;
        console.error(`POST HttpRequest to CohesiveRP backend Webapi failed. Response Error Code=[${exception?.code}], Message=[${exception?.message}].`);
        return result;
      }
      
      console.log("POST HttpRequest to CohesiveRP backend Webapi was successful. Raw response:", JSON.stringify(result));
      return result;
    } catch (err) {
      console.error(`Unhandled error on postToServerApiAsync function in HttpRequestHelper. err=[${err}].`);
    }

    return null;
}
