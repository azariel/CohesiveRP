using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;
using CohesiveRP.Common.Exceptions;

namespace CohesiveRP.Storage.WebApi.Middlewares
{
    /// <summary>
    /// Converts the Http exception to specific http error output.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class StorageWebApiExceptionMiddleware
    {
        private readonly RequestDelegate next;

        /// <param name="next">Instance of the next action delegate to be executed.</param>
        public StorageWebApiExceptionMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        /// <summary>
        /// Gets executed on http requests when this middleware is used.
        /// </summary>
        /// <param name="context">The instance of the .net core application context.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task Invoke(HttpContext context)
        {
#pragma warning disable CA1303 // Do not pass literals as localized parameters
            try
            {
                await this.next.Invoke(context).ConfigureAwait(false);
            }

            //catch (InvalidIdFormatException exception)
            //{
            //    this.logger.LogWarning(exception, $"{exception.Message}");
            //    throw new BadRequestException(exception.Message, VoicechatHistoryFetcherErrorCodes.VoicechatExceptionMiddlewareInvalidIdFormat);
            //}
            catch (ApiException exception)
            {
                context.Response.ContentType = "text/plain";
                context.Response.StatusCode = (int)exception.Code;

                var data = Encoding.UTF8.GetBytes(exception.Message);
                await context.Response.Body.WriteAsync(data, 0, data.Length);
            }
            // Unhandled exception
            catch (Exception exception)
            {
                context.Response.ContentType = "text/plain";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                var data = Encoding.UTF8.GetBytes(exception.Message);
                await context.Response.Body.WriteAsync(data, 0, data.Length);
            }
#pragma warning restore CA1303 // Do not pass literals as localized parameters
        }
    }
}
