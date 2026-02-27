import type { ServerApiExceptionResponseDto } from "../../ResponsesDto/Exceptions/ServerApiExceptionResponseDto";
import type { ServerApiResponseDto } from "../../ResponsesDto/ServerApiResponseDto";
import { extractServerApiResponseFromBody } from "./response/HttpResponseUtils";

const ServerApiUrlPrefix = "https://127.0.0.1:7080/";

// export async function getFromServerApiAsync(): Promise<void> {
//     return;
// }

export async function postToServerApiAsync(url:string, body: any): Promise<ServerApiResponseDto | null> {
    try {
      const response = await fetch(`${ServerApiUrlPrefix}${url}`, {
        method: "POST",
        body: JSON.stringify(body),
      });

      let result:ServerApiResponseDto | null = await extractServerApiResponseFromBody(response.body);
      if (!response.ok) {
        // TODO: notification to user
        let exception: ServerApiExceptionResponseDto | null = result as ServerApiExceptionResponseDto;
        console.log(`Failed to send Player message to backend. Response Error Code=[${exception?.code}], Message=[${exception?.message}].`);
        return exception;
      }
      
      console.log("PlayerMessage sent to server. Response:", JSON.stringify(result));
      return result;
    } catch (err) {
      console.error(`Unhandled error on postToServerApiAsync function in HttpRequestHelper. err=[${err}].`);
    }

    return null;
}
