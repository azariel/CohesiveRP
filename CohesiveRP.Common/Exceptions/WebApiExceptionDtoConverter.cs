namespace CohesiveRP.Common.Exceptions
{
    public static class WebApiExceptionDtoConverter
    {
        public static WebApiException ConvertToWebApiExceptionDto(this ApiException ex)
        {
            return new WebApiException
            {
                Message = ex.Message,
                HttpResultCode = ex.Code,
            };
        }

        public static WebApiException ConvertToWebApiExceptionDto(this Exception ex)
        {
            return new WebApiException
            {
                Message = ex.Message,
                //StackTrace = ex.StackTrace,
                //Type = ex.GetType().FullName
            };
        }
    }
}
