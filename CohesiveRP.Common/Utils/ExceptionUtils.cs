namespace CohesiveRP.Common.Utils
{
    /// <summary>
    /// Helping functions relative to exception
    /// </summary>
    public static class ExceptionUtils
    {
        public static string BuildExceptionAndInnerExceptionsMessage(Exception exception, int spacingLevel = 1)
        {
            if (exception == null)
                return null;

            string _Message = $"{"".PadLeft(spacingLevel)}[{Environment.NewLine}{"".PadLeft(spacingLevel)} Message=[{exception.Message}]{Environment.NewLine} {"".PadLeft(spacingLevel)}StackTrace=[{exception.StackTrace}]{Environment.NewLine}{"".PadLeft(spacingLevel)}]{Environment.NewLine}";

            if (exception.InnerException != null)
                _Message += $"{"".PadLeft(spacingLevel)}InnerException={Environment.NewLine}{"".PadLeft(spacingLevel)}[{Environment.NewLine}{BuildExceptionAndInnerExceptionsMessage(exception.InnerException, spacingLevel + 1)}{"".PadLeft(spacingLevel)}]{Environment.NewLine}";

            return _Message;
        }
    }
}
